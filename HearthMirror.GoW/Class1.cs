using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HearthMirror.Mono;

namespace HearthMirror.GoW
{
    public class Gems
    {
        private static readonly Lazy<Mirror> LazyMirror = new Lazy<Mirror>(() => new Mirror { ImageName = "GemsOfWar" });
        private static Mirror Mirror => LazyMirror.Value;

        public static event Action<Exception> Exception;

        private static T TryGetInternal<T>(Func<T> action, bool clearCache = true)
        {
            try
            {
                var proc = Mirror.Proc;
                if (proc == null)
                    return default(T);
                if (proc.HasExited)
                    Mirror.Clean();
                if (clearCache)
                    Mirror.View?.ClearCache();
                return action.Invoke();
            }
            catch (Exception e)
            {
                Mirror.Clean();
                try
                {
                    return action.Invoke();
                }
                catch (Exception e2)
                {
                    Exception?.Invoke(e2);
                    return default(T);
                }
            }
        }

        public GameState GetGameState() => TryGetInternal(() =>
        {
            var stack = Mirror.Root?["GWMenuSuperSystem"]["_inst"]["PrimaryMenuSystem"]["<MenuStack>k__BackingField"];
            var items = stack["_items"];
            int size = stack["_size"];
            var ui = Enumerable.Range(0, size)
                .Select(i => items[i])
                .FirstOrDefault(c => c.Class.Name == "PuzzleUI");

            if (ui == null)
                return null;

            var players = ui["Players"];

            return new GameState
            {
                UserTeam = GetPlayerTroops(players[0]),
                EnemyTeam = GetPlayerTroops(players[1]),
                Board = GetBoard(ui),
            };
        });

        private GemColor[][] GetBoard(MonoObject ui)
        {
            var gems = ui["Gems"];

            return Enumerable.Range(0, 8)
                .Select(i => Enumerable.Range(0, 8)
                    .Select(j => (GemColor)gems[j][i]["GemColor"])
                    .Reverse()
                    .ToArray())
                .ToArray();
        }

        private IEnumerable<Troop> GetPlayerTroops(MonoObject player)
        {
            var troops = player["Troops"];
            var items = troops["_items"];
            int size = troops["_size"];
            return Enumerable.Range(0, size)
                .Select(i => items[i])
                .Select(t => new Troop
                {
                    Health = t["Health"],
                    Attack = t["Attack"],
                    Armor = t["Armor"],
                    Magic = t["SpellPower"],
                    Mana = t["Mana"],
                    ManaRequired = t["ManaRequired"],
                    SpellReady = t["SpellReady"],
                    Id = t["<Id>k__BackingField"] != 1 ? t["<Id>k__BackingField"] : t["<TroopStats>k__BackingField"]["HeroDataProgress"]["<WeaponOverride>k__BackingField"],
                    StatusEffects = Enumerable.Range(0, 20)
                        .Where(i => t["StatusEffectCounters"][i] > 0)
                        .Select(i => (StatusEffect)i).ToList()
                }).ToList();
        }

        public dynamic GetSuperSystem() => TryGetInternal(() =>
        {
            //var target = Mirror.Root?["GWMenuSuperSystem"]["_inst"]["PrimaryMenuSystem"]["<MenuStack>k__BackingField"]["_items"][4].DebugFields;
            var sys = Mirror.Root?["GWMenuSuperSystem"];
            var inst = sys["_inst"];
            var menus = new[]{
            "BackgroundMenuSystem",
            "PrimaryMenuSystem",
            "OverlayMenuSystem",
            "LoadingMenuSystem",
            "PopupMenuSystem",
            "ChatMenuSystem",
            "ErrorMenuSystem",
            "DebugMenuSystem",
            "WebViewMenuSystem"
                };

            foreach (var menu in menus)
            {
                var m = inst[menu];
                var stack = m["<MenuStack>k__BackingField"];

                var items = stack["_items"];
                if (items == null)
                    continue;

                int size = stack["_size"];
                for (var i = 0; i < size; i++)
                {
                    var el = items[i];
                }
            }

            return inst;
        });
    }
    public class GameState
    {
        public IEnumerable<Troop> UserTeam { get; set; }
        public IEnumerable<Troop> EnemyTeam { get; set; }
        public GemColor[][] Board { get; set; }
    }

    public class Troop
    {
        public int Id { get; set; }
        public int Health { get; set; }
        public int Attack { get; set; }
        public int Armor { get; set; }
        public int Magic { get; set; }
        public int Mana { get; set; }
        public int ManaRequired { get; set; }
        public bool SpellReady { get; set; }
        public IEnumerable<StatusEffect> StatusEffects { get; set; }
    }

    public enum StatusEffect
    {
        Entangle = 0,
        Burning = 1,
        Poison = 2,
        Silence = 3,
        HuntersMark = 4,
        SpellDisabled = 5,
        Barrier = 6,
        Frozen = 7,
        DeathMark = 8,
        Disease = 9,
        Stun = 10,
        Web = 11,
        Enchanted = 12,
        Enraged = 13,
        Submerged = 14,
        FaerieFire = 15,
        Blessed = 16,
        Cursed = 17,
        Bleed = 18,
        Mirror = 19,
    }

    public enum GemColor
    {
        Blue = 0,
        Green = 1,
        Red = 2,
        Yellow = 3,
        Purple = 4,
        Brown = 5,
        Skull = 6,
        Doomskull = 7,
        Block = 8,
        BaseGemTypes = 9,
        LootBronze = 10,
        LootSilver = 11,
        LootGold = 12,
        LootBag = 13,
        LootChest0 = 14,
        LootChest1 = 15,
        LootChest2 = 16,
        LootSafe = 17,
    }

    //public bool IsBusy
    //{
    //    get
    //    {
    //        if (!this.SwappingGems && this.IsPlayerTurn && (!this.m_bStartedMove && !this.m_bStartTurnRunning))
    //            return !this.m_bGameStarted;
    //        return true;
    //    }
    //}
}
