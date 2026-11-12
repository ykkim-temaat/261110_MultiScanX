using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MultiScanX.Core.Models;

namespace MultiScanX.Core.Services
{
    /// <summary>
    /// 검사 레시피 JSON 입출력, 10x10 격자 생성, 행/열/전체 미세조정 및 최단경로(Snake) 정렬 서비스
    /// </summary>
    public class RecipeService
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public Recipe LoadRecipe(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"레시피 파일을 찾을 수 없습니다: {filePath}");
            }

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Recipe>(json, JsonOptions) 
                   ?? throw new InvalidOperationException("레시피 역직렬화 실패");
        }

        public void SaveRecipe(Recipe recipe, string filePath)
        {
            recipe.ModifiedAt = DateTime.Now;
            string dir = Path.GetDirectoryName(filePath) ?? string.Empty;
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string json = JsonSerializer.Serialize(recipe, JsonOptions);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// 510x515mm 대형 기판용 기본 30 Point (6행 x 5열) 균일 격자 레시피 생성기
        /// </summary>
        public Recipe CreateDefault30PointRecipe()
        {
            return CreateGridRecipe("KCT_Default_510x515_30Pt", 510.0, 515.0, 6, 5, 35.0, 35.0);
        }

        /// <summary>
        /// 510x515mm 대형 기판용 10x10 (100 Point) 정밀 매트릭스 레시피 생성기
        /// </summary>
        public Recipe Create10x10GridRecipe()
        {
            return CreateGridRecipe("KCT_Master_510x515_10x10_100Pt", 510.0, 515.0, 10, 10, 25.0, 25.0);
        }

        public Recipe CreateGridRecipe(string name, double width, double height, int rows, int cols, double marginX, double marginY)
        {
            var recipe = new Recipe
            {
                RecipeName = name,
                Description = $"{width}×{height}mm 기판 {rows}×{cols} ({rows * cols}pt) 정밀 매핑 레시피",
                SubstrateWidth = width,
                SubstrateHeight = height,
                GridRows = rows,
                GridCols = cols
            };

            double stepX = cols > 1 ? (width - marginX * 2.0) / (cols - 1) : 0;
            double stepY = rows > 1 ? (height - marginY * 2.0) / (rows - 1) : 0;

            int id = 1;
            for (int r = 1; r <= rows; r++)
            {
                for (int c = 1; c <= cols; c++)
                {
                    // 지그재그(Snake) 경로 기본 적용: 짝수 행은 역방향
                    int actualCol = (r % 2 == 1) ? c : (cols - c + 1);
                    recipe.Points.Add(new InspectionPoint
                    {
                        PointId = id++,
                        RowIndex = r,
                        ColIndex = actualCol,
                        X = Math.Round(marginX + (actualCol - 1) * stepX, 1),
                        Y = Math.Round(marginY + (r - 1) * stepY, 1),
                        TargetZ = 18.400,
                        RepeatCount = 1,
                        IsEnabled = true,
                        EnableMmsCu = true,
                        EnableF20Film = true,
                        EnableTtvLaser = true
                    });
                }
            }

            return recipe;
        }

        /// <summary>
        /// 특정 행(Row), 특정 열(Col) 또는 전체(All) 포인트를 dX, dY, dZ만큼 일괄 미세 이동
        /// </summary>
        public int ShiftPoints(Recipe recipe, int? targetRow, int? targetCol, double dx, double dy, double dz = 0.0)
        {
            int count = 0;
            foreach (var pt in recipe.Points)
            {
                if (targetRow.HasValue && pt.RowIndex != targetRow.Value) continue;
                if (targetCol.HasValue && pt.ColIndex != targetCol.Value) continue;

                pt.X = Math.Clamp(Math.Round(pt.X + dx, 1), 0.0, recipe.SubstrateWidth);
                pt.Y = Math.Clamp(Math.Round(pt.Y + dy, 1), 0.0, recipe.SubstrateHeight);
                pt.TargetZ = Math.Clamp(Math.Round(pt.TargetZ + dz, 3), 0.0, 50.0);
                count++;
            }
            recipe.ModifiedAt = DateTime.Now;
            return count;
        }

        /// <summary>
        /// Snake(지그재그) 최단 이동 경로 재정렬 및 총 이동 거리(mm) 계산
        /// </summary>
        public (double beforeDistMm, double afterDistMm) OptimizeSnakeOrder(Recipe recipe)
        {
            double beforeDist = CalculateTotalTravelDistance(recipe.Points);

            var sorted = recipe.Points
                .OrderBy(p => p.RowIndex)
                .ThenBy(p => p.RowIndex % 2 == 1 ? p.X : -p.X)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                sorted[i].PointId = i + 1;
            }

            recipe.Points = sorted;
            double afterDist = CalculateTotalTravelDistance(recipe.Points);
            return (Math.Round(beforeDist, 1), Math.Round(afterDist, 1));
        }

        public double CalculateTotalTravelDistance(IList<InspectionPoint> points)
        {
            var active = points.Where(p => p.IsEnabled).ToList();
            if (active.Count <= 1) return 0.0;

            double sum = 0.0;
            for (int i = 1; i < active.Count; i++)
            {
                double dx = active[i].X - active[i - 1].X;
                double dy = active[i].Y - active[i - 1].Y;
                sum += Math.Sqrt(dx * dx + dy * dy);
            }
            return sum;
        }
    }
}
