using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using ZoningToolkit.Systems;

namespace ZoningToolkit
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(ZoningToolkit)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        public const string ModName = "ZoningToolkit";
        
        public static Mod Instance { get; private set;  }
        
        /// <summary>
        /// Gets the mod's active settings configuration.
        /// </summary>
        internal ModSettings ActiveSettings { get; private set; }

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            Instance = this;

            ActiveSettings = new ModSettings(this);
            ActiveSettings.RegisterKeyBindings();

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");

            // create systems but don't keep fields for them
            _ = new ZoningToolkitModSystem();
            _ = new ZoningToolkitModUISystem();
            _ = new ZoningToolkitModToolSystem();

            updateSystem.UpdateAt<ZoningToolkitModToolSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<ZoningToolkitModSystem>(SystemUpdatePhase.Modification4B);
            updateSystem.UpdateAt<ZoningToolkitModUISystem>(SystemUpdatePhase.UIUpdate);
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
            if (ActiveSettings != null)
            {
                ActiveSettings.UnregisterInOptionsUI();
                ActiveSettings = null;
            }
        }
    }
}
