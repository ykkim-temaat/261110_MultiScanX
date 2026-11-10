using System;
using System.IO;
using System.Text.Json;
using MultiScanX.Core.Models;

namespace MultiScanX.Core.Services
{
    /// <summary>
    /// 검사 레시피 JSON 입출력 서비스
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
        /// 510x515mm 대형 기판용 기본 30 Point 균일 격자 레시피 생성기
        /// </summary>
        public Recipe CreateDefault30PointRecipe()
        {
            var recipe = new Recipe
            {
                RecipeName = "KCT_Default_510x515_30Pt",
                Description = "510x515mm 대형 기판 30 포인트 균일 매핑 레시피",
                SubstrateWidth = 510.0,
                SubstrateHeight = 515.0
            };

            // 5열 x 6행 = 30 포인트 생성 (여백: X 35mm, Y 35mm)
            int rows = 6;
            int cols = 5;
            double stepX = (510.0 - 70.0) / (cols - 1); // 약 110mm 간격
            double stepY = (515.0 - 70.0) / (rows - 1); // 약 89mm 간격

            int id = 1;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    recipe.Points.Add(new InspectionPoint
                    {
                        PointId = id++,
                        X = Math.Round(35.0 + c * stepX, 3),
                        Y = Math.Round(35.0 + r * stepY, 3),
                        RepeatCount = 3,
                        EnableMmsCu = true,
                        EnableF20Film = true,
                        EnableTtvLaser = true
                    });
                }
            }

            return recipe;
        }
    }
}

