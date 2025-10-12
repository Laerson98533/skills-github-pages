# Quiz Rush - Unity 2022 LTS

Quiz Rush é um jogo mobile de perguntas rápidas criado em Unity 2022 LTS com suporte a Android, monetização via Google Mobile Ads (AdMob) e dataset local de ~200 perguntas PT-BR/PT-PT. O projeto está organizado para sessões curtas de 10 questões com temporizador de 10 segundos, pontuação com streak, leaderboard local, anúncios banner/interstitial/rewarded e testes automatizados.

## Estrutura principal

```
Assets/
  Resources/
    AdConfig.asset            # Configuração padrão dos anúncios (IDs placeholder)
    Prefabs/                  # Prefabs gerados em runtime (Menu, Quiz, Resultado)
    questions.json            # Dataset de perguntas (~200 entradas)
  Scenes/
    Boot.unity                # Cena de arranque (carrega Main)
    Main.unity                # Cena principal, restante conteúdo é instanciado em runtime
  Scripts/
    Ads/                      # Serviços AdMob (Banner, Interstitial, Rewarded, Config, Inicialização)
    Services/                 # Persistência, Analytics e suporte geral
    UI/                       # Controlos de UI (MenuUI, QuizUI, ResultUI)
    GameManager.cs            # Máquina de estados principal
    QuizController.cs         # Lógica das rondas, timer e recompensa
    RuntimeBootstrapper.cs    # Garante que sistemas essenciais existem mesmo em cenas vazias
    ...
  Tests/
    EditMode/ScoreSystemTests.cs
    PlayMode/QuestionRepositoryTests.cs
```

## Como integrar o Google Mobile Ads Unity Plugin

1. **Instalar o SDK**
   - Baixe o [Google Mobile Ads Unity plugin](https://developers.google.com/admob/unity/quick-start#download_the_google_mobile_ads_sdk) compatível com Unity 2022 LTS.
   - Importe o `.unitypackage` no projeto (`Assets > Import Package > Custom Package`).
   - Verifique se a pasta `GoogleMobileAds/` foi adicionada e se a definição de scripting `GOOGLE_MOBILE_ADS` está ativa (Unity adiciona automaticamente, caso contrário adicione em `Project Settings > Player > Scripting Define Symbols`).

2. **Atualizar os IDs de anúncios**
   - Abra o asset `Assets/Resources/AdConfig.asset` e substitua os valores placeholder pelos IDs reais fornecidos pela AdMob:
     ```
     BANNER_AD_UNIT_ID = "ca-app-pub-xxx/banner"
     INTERSTITIAL_AD_UNIT_ID = "ca-app-pub-xxx/interstitial"
     REWARDED_AD_UNIT_ID = "ca-app-pub-xxx/rewarded"
     ```
   - Se preferir, utilize a janela `Tools/Quiz Rush/AdMob Ids` (Editor) para editar os IDs e toggles de forma visual.
   - Em tempo de execução os serviços carregam esse asset automaticamente via `Resources.Load`.

3. **Conferir o AdMobInitializer**
   - `RuntimeBootstrapper` cria um GameObject com `AdMobInitializer` na cena Boot. Este script chama `MobileAds.Initialize` e respeita as flags configuradas em `AdConfig`.

## Fluxo das cenas e anúncios

- **Boot**: cena minimalista que, ao carregar, instância `BootLoader` + `AdMobInitializer` e muda para `Main` após um pequeno splash.
- **Main**: `RuntimeBootstrapper` cria `GameSystems`, adicionando `GameManager`, `QuestionRepository`, serviços de anúncios e analítica. O `GameManager` gera dinamicamente um Canvas responsivo e instancia os prefabs de UI.
- **Anúncios**:
  - **Banner**: carregado apenas no Menu/Hub (`AdBannerService.Show()` quando o menu está ativo).
  - **Interstitial**: exibido a cada duas rondas concluídas, com intervalo mínimo de 120 segundos entre impressões (`AdInterstitialService`).
  - **Rewarded**: oferecido uma vez por ronda ao falhar/terminar o tempo (`AdRewardedService.ShowForExtraLife`).

## Build Android (APK/AAB)

1. Abra o projeto no Unity 2022 LTS.
2. Configure o Android SDK/NDK na primeira abertura se necessário (`Unity Hub > Installs > Add Modules`).
3. Ajuste as Player Settings:
   - `File > Build Settings > Android > Switch Platform`.
   - Em `Player Settings`, defina o package name (e.g., `com.seuprojeto.quizrush`), versão, ícones e ativações desejadas.
4. Em `Build Settings`, escolha `Build` (APK) ou `Build and Run`. Para AAB utilize `Build > Google Android Project` ou o fluxo `Build App Bundle` do Unity.
5. O jogo utiliza `Resources.Load` e geração dinâmica de UI; não são necessários ajustes adicionais para cenas.

## Boas práticas de anúncios (AdMob)

- **Banner**: nunca exibido durante o gameplay, apenas no menu principal.
- **Interstitial**: respeita frequência mínima de 2 rondas e intervalo de 120 segundos.
- **Rewarded**: opcional, somente quando o jogador falha uma pergunta e deseja ganhar +1 vida (máximo 1 por ronda).
- **Fallback seguro**: se algum anúncio falhar ao carregar, o jogo continua normalmente.

## Highscore, streak e dataset

- `questions.json` contém 204 entradas distribuídas igualmente entre as categorias Cultura Geral, Geografia, Cinema, Desporto, Ciência e Emoji-quiz. Cada pergunta possui 4 opções, índice correto e dificuldade de 1 a 5.
- `QuestionRepository` evita repetir perguntas dentro da mesma ronda até esgotar o pool.
- `ScoreSystem` calcula `score = 100 * multiplier + timeBonus`, com multiplicador crescendo +0.2 por acerto consecutivo.
- `PersistenceService` usa `PlayerPrefs` para armazenar top 10 geral, melhor pontuação por categoria, preferências de som e vibração.

## Testes automatizados

- **EditMode**: `ScoreSystemTests` valida o comportamento de streak/multiplicador.
- **PlayMode**: `QuestionRepositoryTests` garante que não há repetição dentro da mesma ronda antes de esgotar o conjunto.
- Execute a partir do Unity Test Runner (`Window > General > Test Runner`).

## Extensões e integração futura

- `AnalyticsHook` regista eventos (`session_start`, `round_start`, `round_end`, `ad_*`) e mantém fila curta de logs; pode ser adaptado para Firebase/Analytics externos.
- `AdConfig` inclui toggles para ativar/desativar anúncios no editor sem alterar código.
- `RuntimeBootstrapper` e `GameManager` foram desenhados para facilitar testes em Editor sem depender de cena pré-configurada.

## Créditos e licença

- Projeto preparado para servir como base de monetização rápida em jogos trivia.
- Licenciado sob [MIT License](LICENSE).
