using System;
using System.IO;
using System.Text.Json;
using MultiScanX.Core.Models;

namespace MultiScanX.Core.Services
{
    /// <summary>
    /// 설비 런타임 설정(system_config.json) 로드 및 자동 초기화 서비스
    /// </summary>
    public class SystemConfigService
    {
        public const string DefaultConfigPath = @"C:\MultiScanX\config\system_config.json";

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public string ActiveConfigPath { get; private set; }

        public SystemConfigService(string? configPath = null)
        {
            ActiveConfigPath = string.IsNullOrWhiteSpace(configPath) ? DefaultConfigPath : configPath;
        }

        /// <summary>
        /// 설정 파일을 로드하며, 파일이 없을 경우 기본 설정 파일을 자동 생성합니다.
        /// 기본 경로 쓰기 권한이 제한될 경우 LocalAppData 하위 경로로 폴백합니다.
        /// </summary>
        public SystemConfig LoadOrCreate()
        {
            // 1. 기본/지정 경로에 파일이 존재하면 우선 읽기 시도
            if (File.Exists(ActiveConfigPath))
            {
                return DeserializeFromFile(ActiveConfigPath);
            }

            // 2. 폴백 경로에 기존 설정이 있다면 그 경로 사용
            string fallbackPath = GetFallbackConfigPath();
            if (File.Exists(fallbackPath))
            {
                ActiveConfigPath = fallbackPath;
                return DeserializeFromFile(fallbackPath);
            }

            // 3. 신규 기본 설정 생성 (D: 드라이브 유무 반영)
            var defaultConfig = CreateDefaultConfig();
            try
            {
                Save(defaultConfig, ActiveConfigPath);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
            {
                // C:\MultiScanX\config 쓰기 권한 없을 때 사용자 로컬 경로로 안전하게 폴백
                ActiveConfigPath = fallbackPath;
                try
                {
                    Save(defaultConfig, ActiveConfigPath);
                }
                catch
                {
                    // 파일 저장에 실패해도 메모리 내 기본값으로 정상 부팅 보장
                }
            }

            return defaultConfig;
        }

        public void Save(SystemConfig config, string? targetPath = null)
        {
            string path = string.IsNullOrWhiteSpace(targetPath) ? ActiveConfigPath : targetPath;
            string dir = Path.GetDirectoryName(path) ?? string.Empty;
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(path, json);
        }

        private static SystemConfig DeserializeFromFile(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<SystemConfig>(json, JsonOptions) ?? CreateDefaultConfig();
            }
            catch
            {
                return CreateDefaultConfig();
            }
        }

        private static SystemConfig CreateDefaultConfig()
        {
            string defaultDataRoot = Directory.Exists(@"D:\") ? @"D:\MultiScanX_Data" : @"C:\MultiScanX_Data";
            return new SystemConfig
            {
                DataRootPath = defaultDataRoot
            };
        }

        private static string GetFallbackConfigPath()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, "MultiScanX", "config", "system_config.json");
        }
    }
}

