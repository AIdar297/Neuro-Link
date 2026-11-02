using System.Collections.Generic;

namespace Neuro_Link.Scenes
{
    using Neuro_Link.Engine;
    using Neuro_Link.Engine;

    public static class SceneDatabase
    {
        private static Dictionary<string, Scene> _scenes;

        public static Scene Get(string id) => _scenes.ContainsKey(id) ? _scenes[id] : null;

        static SceneDatabase()
        {
            _scenes = new Dictionary<string, Scene>();

            // ===== АКТ 1: ПРОБУЖДЕНИЕ =====
            Add("intro", "Твоя квартира в Нижнем городе. Неоновые вывески мигают за окном. Голова раскалывается. На полу... тело.",
                new Dialogue("Newton", "Черт... что за... Эли?! ЭЛИ! Нет... нет, нет, нет... На моих руках кровь. Я ничего не помню. Последние 48 часов — пустота."),
                new List<Choice> { new Choice { Text = "Осмотреть тело", NextScene = "act1_examine" }, new Choice { Text = "Посмотреть в окно", NextScene = "act1_window" } });

            Add("act1_examine", "Ты опускаешься на колени рядом с Эли. На её шее — следы от нейро-считывателя. Её имплант выжжен.",
                new Dialogue("Newton", "Её нейро-линк выжжен изнутри. Так делают только в CORTEXIS. 'Протокол забвения'... Я слышал об этом от неё. Они стирают людей."),
                new List<Choice> { new Choice { Text = "Забрать её чип памяти", NextScene = "act1_chip", SetFlag = "has_chip" }, new Choice { Text = "Бежать отсюда", NextScene = "act1_escape" } });

            Add("act1_window", "За окном — Нижний город. Дождь. Неоновые иероглифы. И три чёрных фургона CORTEXIS у твоего подъезда.",
                new Dialogue("Newton", "Зачистка. Уже здесь. У меня минут пять, может меньше. Надо валить.")),
                new List<Choice> { new Choice { Text = "Быстро схватить чип Эли", NextScene = "act1_chip", SetFlag = "has_chip" }, new Choice { Text = "Бежать без ничего", NextScene = "act1_escape" } });

            Add("act1_chip", "Ты аккуратно извлекаешь чип из её шеи. Он тёплый. Внутри что-то светится.",
                new Dialogue("Newton", "Что ты успела узнать, Эли... Ладно. Сейчас не время горевать. Надо выбираться.")),
                new List<Choice> { new Choice { Text = "На пожарную лестницу", NextScene = "act1_qte" } });

            Add("act1_escape", "Ты хватаешь куртку и бросайся к двери.",
                new Dialogue("Newton", "Нет времени. Только вперед.")),
                new List<Choice> { new Choice { Text = "На пожарную лестницу", NextScene = "act1_qte" } });

            Add("act1_qte", "Ты вырываешься на пожарную лестницу. Внизу слышны голоса зачистки CORTEXIS. Они поднимаются.",
                new Dialogue("Newton", "Быстрее! Нужно перепрыгнуть на соседнюю крышу!"),
                null, SceneType.QTE, "act1_rooftop", "act1_qte");

            Add("act1_rooftop", "Ты на крыше. Дождь хлещет в лицо. Внизу мигают синие огни CORTEXIS.",
                new Dialogue("Newton", "Фух... Отстанем на время. Нужно спуститься в Нижний город. Найти Крысу. Он знает всё обо всём.")),
                new List<Choice> { new Choice { Text = "Спуститься в Нижний город", NextScene = "act2_start" } });

            // ===== АКТ 2: НИЖНИЙ ГОРОД =====
            Add("act2_start", "Нижний город. Узкие переулки, залитые неоном. Пахнет синтетикой и озоном. Ты идёшь к бару 'Чёрный пиксель'.",
                new Dialogue("Newton", "Крыса должен быть там. Если он ещё жив.")),
                new List<Choice> { new Choice { Text = "Войти в бар", NextScene = "act2_bar" } });

            Add("act2_bar", "В баре полумрак. В углу сидит тощий парень с кибер-глазом. Это Крыса.",
                new Dialogue("Rat", "Ньют?! Ты жив?! Слух по сети — тебя CORTEXIS уже в утиль отправила. Что случилось?")),
                new List<Choice> {
                    new Choice { Text = "Рассказать про Эли", NextScene = "act2_tell" },
                    new Choice { Text = "Сначала требовать информацию", NextScene = "act2_demand" }
                });

            Add("act2_tell", "Ты рассказываешь Крысе всё. Он бледнеет.",
                new Dialogue("Rat", "Протокол Омега... Черт, Ньют. Эли не просто умерла. Её сознание оцифровали. Она сейчас в сети CORTEXIS. Живая. В каком-то смысле.")),
                new List<Choice> { new Choice { Text = "Как её вытащить?", NextScene = "act2_plan" } });

            Add("act2_demand", "Ты хватаешь Крысу за воротник.",
                new Dialogue("Newton", "Мне не до болтовни. Что ты знаешь о Протоколе Омега?"), new Dialogue("Rat", "Эй, полегче! ...Ладно. Эли жива. В сети. CORTEXIS оцифровали её.")),
                new List<Choice> { new Choice { Text = "Как её вытащить?", NextScene = "act2_plan" } });

            Add("act2_plan", "Крыса рисует на салфетке схему.",
                new Dialogue("Rat", "Дата-центр CORTEXIS в секторе 7. Охрана — кибер-солдаты. Но есть слепая зона в 3 минуты. Я дам тебе код доступа. Дальше — сам.")),
                new List<Choice> { new Choice { Text = "Согласиться", NextScene = "act2_ambush" } });

            Add("act2_ambush", "Вы выходите из бара. Из тени выходят трое бандитов с арм-пушками.",
                new Dialogue("Newton", "Крыса, это твои друзья?"), new Dialogue("Rat", "Нет... это охотники CORTEXIS! Бей их!")),
                new List<Choice> { new Choice { Text = "В БОЙ!", NextScene = "act2_combat" } });

            Add("act2_combat", "Переулок. Трое кибер-бандитов против тебя.",
                null, null, SceneType.COMBAT, "act2_aftermath", "act2_combat");

            Add("act2_aftermath", "Бандиты лежат. Ты ранен, но жив.",
                new Dialogue("Rat", "Черт, Ньют, ты зверь. Держи — ключ от черного хода дата-центра. Удачи.")),
                new List<Choice> { new Choice { Text = "Идти к дата-центру", NextScene = "act3_start" } });

            // ===== АКТ 3: ПРОНИКНОВЕНИЕ =====
            Add("act3_start", "Дата-центр CORTEXIS. Серая башня, пронзающая небо. Ты у черного хода.",
                new Dialogue("Newton", "Так. Сначала нужно взломать терминал охраны.")),
                new List<Choice> { new Choice { Text = "Начать взлом", NextScene = "act3_hack" } });

            Add("act3_hack", "Терминал мигает красным. Нужно подобрать 4-значный код.",
                null, null, SceneType.HACK, "act3_inside", "act3_alarm");

            Add("act3_alarm", "Сирена. Красные огни. Ты не успел.",
                new Dialogue("Newton", "Черт! Придется идти через главный зал. Там кибер-солдаты.")),
                new List<Choice> { new Choice { Text = "В бой через главный зал", NextScene = "act3_elite" } });

            Add("act3_inside", "Дверь со скрипом открывается. Ты внутри. Ряды серверов гудят, как живые.",
                new Dialogue("Newton", "Так... Где тут ядро?")),
                new List<Choice> { new Choice { Text = "Идти к ядру", NextScene = "act3_elite" } });

            Add("act3_elite", "Перед тобой — элитный кибер-солдат CORTEXIS. Черная броня, красный визор.",
                new Dialogue("CORTEXIS", "Объект Ньют. Ликвидация санкционирована.")),
                new List<Choice> { new Choice { Text = "Принять бой", NextScene = "act3_elite_fight" } });

            Add("act3_elite_fight", "Бой с элитным солдатом. Он сильнее тебя.",
                null, null, SceneType.COMBAT, "act3_core", "act3_elite_fight");

            Add("act3_core", "Ты в ядре. Огромный голографический куб пульсирует светом. И вдруг — голос Эли.",
                new Dialogue("Eli", "Ньют... Ты пришел. Я здесь. Они сделали со мной это... Но я всё ещё я.")),
                new List<Choice> {
                    new Choice { Text = "Я вытащу тебя", NextScene = "act4_choice" },
                    new Choice { Text = "Что с тобой сделали?", NextScene = "act3_truth" }
                });

            Add("act3_truth", "Эли рассказывает всё. Протокол Омега — это не стирание. Это оцифровка. CORTEXIS собирает лучших людей, чтобы использовать их разум как ИИ-ресурс.",
                new Dialogue("Eli", "Я нашла доказательства. Они хотели продать технологию военным. Я украла данные. Они поймали меня.")),
                new List<Choice> { new Choice { Text = "Что делать дальше?", NextScene = "act4_choice" } });

            // ===== АКТ 4: РАЗВЯЗКА =====
            Add("act4_choice", "Перед тобой — главный терминал. ИИ CORTEXIS говорит с тобой.",
                new Dialogue("CORTEXIS", "Ньют. Ты впечатлил меня. Я предлагаю сделку. Стань моим партнером. У тебя будет всё. Или... попробуй сломать меня. Но ты проиграешь.")),
                new List<Choice> {
                    new Choice { Text = "🔥 Выгрузить Эли и все данные в сеть (КОНЦОВКА 1)", NextScene = "ending_1" },
                    new Choice { Text = "👔 Принять предложение ИИ (КОНЦОВКА 2)", NextScene = "ending_2" },
                    new Choice { Text = "💤 Подключиться к симуляции с Эли (КОНЦОВКА 3)", NextScene = "ending_3" },
                    new Choice { Text = "💥 Перегрузить реактор (КОНЦОВКА 4)", NextScene = "ending_4" }
                });

            Add("ending_1", "", null, null, SceneType.ENDING, null, null, 0, 0, 1);
            Add("ending_2", "", null, null, SceneType.ENDING, null, null, 0, 0, 2);
            Add("ending_3", "", null, null, SceneType.ENDING, null, null, 0, 0, 3);
            Add("ending_4", "", null, null, SceneType.ENDING, null, null, 0, 0, 4);
        }

        private static void Add(string id, string desc, Dialogue dlg, List<Choice> choices,
            SceneType type = SceneType.DIALOGUE, string success = null, string fail = null, int end = 0, int _ = 0, int endingId = 0)
        {
            _scenes[id] = new Scene
            {
                Id = id,
                Description = desc,
                Dialogue = dlg,
                Choices = choices,
                Type = type,
                SuccessScene = success,
                FailScene = fail,
                EndingId = endingId
            };
        }
    }
}