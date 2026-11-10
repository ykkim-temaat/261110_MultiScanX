using System;
using System.Threading;
using System.Threading.Tasks;
using MultiScanX.Core.Interfaces;
using MultiScanX.Core.Models;

namespace MultiScanX.Core.Services
{
    /// <summary>
    /// 자동 계측 시퀀스 제어 서비스 (30 Point 스캐너 엔진)
    /// </summary>
    public class ScanSequenceService
    {
        private readonly IMotionController _motion;
        private readonly IDigitalIO _dio;
        private readonly IMmsThicknessDriver _mms;
        private readonly IF20FilmDriver _f20;
        private readonly ILoadCellDriver _loadCell;

        public event Action<MeasurementPointResult>? OnPointCompleted;
        public event Action<string>? OnLogMessage;
        public event Action<double>? OnProgressChanged;

        private CancellationTokenSource? _cts;

        public ScanSequenceService(
            IMotionController motion,
            IDigitalIO dio,
            IMmsThicknessDriver mms,
            IF20FilmDriver f20,
            ILoadCellDriver loadCell)
        {
            _motion = motion;
            _dio = dio;
            _mms = mms;
            _f20 = f20;
            _loadCell = loadCell;
        }

        /// <summary>
        /// 레시피 기반 자동 스캔 검사 시작
        /// </summary>
        public async Task<SubstrateScanResult> ExecuteScanAsync(Recipe recipe, string lotId, string substrateId)
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            var result = new SubstrateScanResult
            {
                LotId = lotId,
                SubstrateId = substrateId,
                RecipeName = recipe.RecipeName,
                StartTime = DateTime.Now
            };

            OnLogMessage?.Invoke($"[시퀀스 시작] Lot: {lotId}, Substrate: {substrateId}, 포인트 수: {recipe.Points.Count}");

            try
            {
                // 1단계: 진공 척 흡착 및 클램프 전진
                OnLogMessage?.Invoke("기판 흡착 및 클램핑 시퀀스 수행 중...");
                _dio.SetVacuumZone1(recipe.UseVacuumZone1);
                _dio.SetVacuumZone2(recipe.UseVacuumZone2);
                await Task.Delay(500, token);

                if (!_dio.IsVacuumOk())
                {
                    throw new InvalidOperationException("진공 흡착 압력(-75kPa) 도달 실패! 인터록 정지");
                }

                if (recipe.UseMechanicalClamp)
                {
                    _dio.SetClampForward(true);
                    await Task.Delay(300, token);
                }

                // 2단계: Z축 안전 대기 위치 확인
                await _motion.MovePosAsync(2, recipe.ZRetractHeight, 20.0);

                // 3단계: 포인트 순회 자동 계측
                int totalPoints = recipe.Points.Count;
                int currentIdx = 0;

                foreach (var pt in recipe.Points)
                {
                    token.ThrowIfCancellationRequested();
                    currentIdx++;
                    OnProgressChanged?.Invoke((double)currentIdx / totalPoints * 100.0);
                    OnLogMessage?.Invoke($"[Point {pt.PointId}/{totalPoints}] 이동: X={pt.X:F3}mm, Y={pt.Y:F3}mm");

                    // 3-1. X/Y 목표 좌표 고속 이송
                    var moveXTask = _motion.MovePosAsync(0, pt.X, 150.0);
                    var moveYTask = _motion.MovePosAsync(1, pt.Y, 150.0);
                    await Task.WhenAll(moveXTask, moveYTask);

                    // 3-2. Z축 프로브 하강 및 로드셀 접촉 감지 인터록
                    double contactLoad = await DescendZWithLoadCellInterlockAsync(recipe, token);

                    // 3-3. 현재 물리적 위치 및 스케일 피드백 판독
                    double curZ = _motion.GetLinearScaleZ();

                    // 3-4. 사급 계측기 측정 수행
                    double cuSum = 0;
                    double filmSum = 0;
                    double gofSum = 0;

                    for (int r = 0; r < pt.RepeatCount; r++)
                    {
                        if (pt.EnableMmsCu)
                        {
                            cuSum += await _mms.MeasureCuThicknessUmAsync();
                        }
                        if (pt.EnableF20Film)
                        {
                            var f20Res = await _f20.MeasureFilmAsync(recipe.RecipeName);
                            filmSum += f20Res.thicknessNm;
                            gofSum += f20Res.gof;
                        }
                    }

                    var ptResult = new MeasurementPointResult
                    {
                        PointId = pt.PointId,
                        TargetX = pt.X,
                        TargetY = pt.Y,
                        ActualX = _motion.GetActualPosition(0),
                        ActualY = _motion.GetActualPosition(1),
                        ActualZ = curZ,
                        CuThicknessUm = pt.EnableMmsCu ? cuSum / pt.RepeatCount : 0.0,
                        FilmThicknessNm = pt.EnableF20Film ? filmSum / pt.RepeatCount : 0.0,
                        FilmGoodnessOfFit = pt.EnableF20Film ? gofSum / pt.RepeatCount : 1.0,
                        ContactLoadG = contactLoad,
                        IsPass = true
                    };

                    result.PointResults.Add(ptResult);
                    OnPointCompleted?.Invoke(ptResult);

                    // 3-5. Z축 안전 높이 복귀
                    await _motion.MovePosAsync(2, recipe.ZRetractHeight, recipe.ZApproachSpeed);
                }

                // 4단계: 통계 산출
                CalculateStatistics(result);
                OnLogMessage?.Invoke($"[시퀀스 완료] 평균 Cu: {result.CuAvgUm:F2}um, 평균 박막: {result.FilmAvgNm:F1}nm");
            }
            catch (OperationCanceledException)
            {
                OnLogMessage?.Invoke("[사용자 취소] 계측 시퀀스가 중단되었습니다.");
                _motion.Stop(0); _motion.Stop(1); _motion.Stop(2);
                await _motion.MovePosAsync(2, recipe.ZRetractHeight, 20.0);
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"[시퀀스 에러] {ex.Message}");
                _motion.EmergencyStop(0); _motion.EmergencyStop(1); _motion.EmergencyStop(2);
                throw;
            }
            finally
            {
                result.EndTime = DateTime.Now;
            }

            return result;
        }

        public void CancelScan()
        {
            _cts?.Cancel();
        }

        /// <summary>
        /// Z축 하강 중 CAS 로드셀 하중을 고속 감시하여 30g 도달 즉시 멈추는 정압 접촉 인터록
        /// </summary>
        private async Task<double> DescendZWithLoadCellInterlockAsync(Recipe recipe, CancellationToken token)
        {
            // 1차: 표면 근접 높이까지 고속 접근
            await _motion.MovePosAsync(2, 5.0, recipe.ZApproachSpeed);

            // 2차: 초저속(Creep 0.5mm/s) 하강하면서 10ms 단위로 하중 체크
            _loadCell.Tare(); // 영점 잡기
            var descendTask = _motion.MovePosAsync(2, -2.0, recipe.ZCreepSpeed);

            double finalLoad = 0;
            while (!descendTask.IsCompleted)
            {
                token.ThrowIfCancellationRequested();
                double weight = _loadCell.GetCurrentWeightG();
                finalLoad = weight;

                if (weight >= recipe.ContactTargetWeightG)
                {
                    _motion.Stop(2); // 즉시 Z축 정지
                    break;
                }
                if (weight >= recipe.MaxSafetyWeightG)
                {
                    _motion.EmergencyStop(2);
                    throw new InvalidOperationException($"로드셀 과하중 비상 경보 ({weight:F1}g > {recipe.MaxSafetyWeightG}g)");
                }

                await Task.Delay(10, token);
            }

            return finalLoad;
        }

        private void CalculateStatistics(SubstrateScanResult res)
        {
            if (res.PointResults.Count == 0) return;

            double sumCu = 0, minCu = double.MaxValue, maxCu = double.MinValue;
            double sumFilm = 0;

            foreach (var p in res.PointResults)
            {
                sumCu += p.CuThicknessUm;
                sumFilm += p.FilmThicknessNm;
                if (p.CuThicknessUm < minCu) minCu = p.CuThicknessUm;
                if (p.CuThicknessUm > maxCu) maxCu = p.CuThicknessUm;
            }

            res.CuAvgUm = sumCu / res.PointResults.Count;
            res.CuMinUm = minCu;
            res.CuMaxUm = maxCu;
            res.FilmAvgNm = sumFilm / res.PointResults.Count;
        }
    }
}

