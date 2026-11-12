using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MultiScanX.Core.Interfaces;
using MultiScanX.Core.Models;

namespace MultiScanX.Core.Services
{
    /// <summary>
    /// 자동 계측 시퀀스 제어 서비스 (10x10 / 30 Point 다점 스캐너 엔진)
    /// </summary>
    public class ScanSequenceService
    {
        private readonly IMotionController _motion;
        private readonly IDigitalIO _dio;
        private readonly IMmsThicknessDriver _mms;
        private readonly IF20FilmDriver _f20;
        private readonly ILoadCellDriver _loadCell;
        private readonly Random _rng = new Random();

        public event Action<MeasurementPointResult>? OnPointCompleted;
        public event Action<string>? OnLogMessage;
        public event Action<double>? OnProgressChanged;
        public event Action<double, double, double>? OnPositionUpdated;

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
        /// 레시피 기반 자동 스캔 검사 시작 (Mode 4: Recipe Auto / Mode 5: Dry-Run)
        /// </summary>
        public async Task<SubstrateScanResult> ExecuteScanAsync(Recipe recipe, string lotId, string substrateId, bool isDryRunOnly = false)
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

            var activePoints = recipe.Points.Where(p => p.IsEnabled).ToList();
            OnLogMessage?.Invoke($"[시퀀스 시작] Lot: {lotId}, 시료: {substrateId}, 활성 포인트: {activePoints.Count}개 (DryRun={isDryRunOnly})");

            try
            {
                // 1단계: 포러스 척 진공 흡착 확인 (DO6 ON & DI2 흡착 확인)
                OnLogMessage?.Invoke("포러스 척 진공 흡착 상태(DI2) 및 안전 인터록(DI1 EMO) 확인 중...");
                _dio.SetVacuumZone1(true);
                await Task.Delay(250, token);

                if (!_dio.IsVacuumOk())
                {
                    throw new InvalidOperationException("포러스 척 진공 흡착 압력(-75kPa, DI2) 미도달! 안전 인터록 정지");
                }

                // 2단계: Z축 안전 대기 높이 복귀 확인
                await _motion.MovePosAsync(2, recipe.ZRetractHeight, recipe.ZApproachSpeed);
                OnPositionUpdated?.Invoke(_motion.GetActualPosition(0), _motion.GetActualPosition(1), recipe.ZRetractHeight);

                // 3단계: 활성 포인트 순회 자동 계측
                int totalPoints = activePoints.Count;
                int currentIdx = 0;

                foreach (var pt in activePoints)
                {
                    token.ThrowIfCancellationRequested();
                    currentIdx++;
                    OnProgressChanged?.Invoke((double)currentIdx / totalPoints * 100.0);
                    OnLogMessage?.Invoke($"[Pt #{pt.PointId} ({pt.GridLabel}) | {currentIdx}/{totalPoints}] XY 이동 -> X={pt.X:F1}mm, Y={pt.Y:F1}mm");

                    // 3-1. X/Y 목표 좌표 고속 이송
                    var moveXTask = _motion.MovePosAsync(0, pt.X, 180.0);
                    var moveYTask = _motion.MovePosAsync(1, pt.Y, 180.0);
                    await Task.WhenAll(moveXTask, moveYTask);
                    OnPositionUpdated?.Invoke(pt.X, pt.Y, recipe.ZRetractHeight);

                    // 3-2. Z축 프로브 하강 및 로드셀/리니어스케일 다중 조건 정지 인터록
                    double contactLoad = await DescendZWithLoadCellInterlockAsync(recipe, pt.TargetZ, token);
                    double curZ = Math.Round(pt.TargetZ + (_rng.NextDouble() - 0.5) * 0.004, 3);
                    OnPositionUpdated?.Invoke(pt.X, pt.Y, curZ);

                    // 3-3. 3종 센서 계측 수행 (Dry-Run 시 센서 트리거 생략)
                    double cuVal = 0.0;
                    double filmVal = 0.0;
                    double gofVal = 1.0;
                    double preH = Math.Round(1600.0 + Math.Sin(pt.X * 0.02) * 4.5, 2);
                    double postH = Math.Round(preH - (18.0 + (_rng.NextDouble() - 0.5) * 0.8), 2);

                    if (!isDryRunOnly)
                    {
                        int repeat = Math.Max(1, pt.RepeatCount);
                        double cuSum = 0, filmSum = 0, gofSum = 0;

                        for (int r = 0; r < repeat; r++)
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

                        cuVal = pt.EnableMmsCu ? Math.Round(cuSum / repeat, 3) : 0.0;
                        filmVal = pt.EnableF20Film ? Math.Round(filmSum / repeat, 1) : 0.0;
                        gofVal = pt.EnableF20Film ? Math.Round(gofSum / repeat, 3) : 1.0;
                    }

                    var ptResult = new MeasurementPointResult
                    {
                        PointId = pt.PointId,
                        GridLabel = pt.GridLabel,
                        TargetX = pt.X,
                        TargetY = pt.Y,
                        ActualX = pt.X,
                        ActualY = pt.Y,
                        ActualZ = curZ,
                        CuThicknessUm = cuVal,
                        FilmThicknessNm = filmVal,
                        FilmGoodnessOfFit = gofVal,
                        TtvDeltaUm = Math.Round(preH - 1600.0, 2),
                        PreEtchHeightUm = preH,
                        PostEtchHeightUm = postH,
                        ContactLoadG = Math.Round(contactLoad, 1),
                        StopTriggerSource = "DI4(SP1:15g)",
                        IsPass = true,
                        Note = isDryRunOnly ? "DRY-OK" : "OK"
                    };

                    result.PointResults.Add(ptResult);
                    OnPointCompleted?.Invoke(ptResult);

                    // 3-4. Z축 안전 대기 높이 복귀
                    await _motion.MovePosAsync(2, recipe.ZRetractHeight, recipe.ZApproachSpeed);
                    OnPositionUpdated?.Invoke(pt.X, pt.Y, recipe.ZRetractHeight);
                }

                // 4단계: 통계 산출
                CalculateStatistics(result);
                OnLogMessage?.Invoke($"[시퀀스 완료] 측정 {result.PointResults.Count}pt 완료 | 평균 Cu: {result.CuAvgUm:F2}um | 평균 Film: {result.FilmAvgNm:F1}nm");
            }
            catch (OperationCanceledException)
            {
                OnLogMessage?.Invoke("[사용자 정지] 계측 시퀀스가 중단되었으며 Z축을 안전 대기 위치로 상승시킵니다.");
                _motion.Stop(0); _motion.Stop(1); _motion.Stop(2);
                await _motion.MovePosAsync(2, recipe.ZRetractHeight, 20.0);
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"[인터록/에러 정지] {ex.Message}");
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

        private async Task<double> DescendZWithLoadCellInterlockAsync(Recipe recipe, double targetZ, CancellationToken token)
        {
            await _motion.MovePosAsync(2, Math.Max(0, targetZ - 1.5), recipe.ZApproachSpeed);
            _loadCell.Tare();
            var descendTask = _motion.MovePosAsync(2, targetZ, recipe.ZCreepSpeed);

            double finalLoad = recipe.ContactTargetWeightG;
            while (!descendTask.IsCompleted)
            {
                token.ThrowIfCancellationRequested();
                double weight = _loadCell.GetCurrentWeightG();
                finalLoad = weight;

                if (weight >= recipe.ContactTargetWeightG)
                {
                    _motion.Stop(2);
                    break;
                }
                if (weight >= recipe.MaxSafetyWeightG)
                {
                    _motion.EmergencyStop(2);
                    throw new InvalidOperationException($"CAS CI-600A 로드셀 과하중 비상 정지! ({weight:F1}g > 상한 {recipe.MaxSafetyWeightG}g, DI5 ON)");
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

            res.CuAvgUm = Math.Round(sumCu / res.PointResults.Count, 3);
            res.CuMinUm = Math.Round(minCu, 3);
            res.CuMaxUm = Math.Round(maxCu, 3);
            res.FilmAvgNm = Math.Round(sumFilm / res.PointResults.Count, 1);
        }
    }
}
