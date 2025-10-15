# Aventura no Castelo Sombrio

"Aventura no Castelo Sombrio" é um jogo de exploração textual com interface híbrida:
no console você lê as descrições, digita comandos e acompanha os eventos, enquanto na
interface Swing há botões que disparam as principais ações e uma área de texto que espelha
as mensagens do terminal.

## Recursos principais

- **Narrativa ramificada**: cada sala do castelo possui descrições e conexões exclusivas
  (salão, biblioteca, masmorra, arsenal, torre e portão selado).
- **Sistema de combate por turnos**: enfrente goblins, esqueletos e magos alternando ataques
  com resultados levemente aleatórios.
- **Inventário completo**: colete chaves, armas e poções, consulte o inventário e utilize itens
  pelo console ou pela interface gráfica.
- **Salvamento rápido**: grave um resumo do progresso em `savegame.txt` a qualquer momento.
- **Interface Swing opcional**: acompanhe a aventura visualmente e utilize botões para explorar,
  lutar, coletar itens, salvar e muito mais.

## Estrutura do código

O código-fonte está organizado no diretório `src/aventura` e utiliza classes orientadas a
objetos para representar cada conceito central:

- `Main`: ponto de entrada, prepara o jogador, inicia a interface Swing e o laço do jogo.
- `Game`: controla o fluxo da aventura, os menus, o combate e a integração console/Swing.
- `Player`: armazena estado do herói (vida, inventário e bônus de ataque).
- `Enemy`: modela inimigos com vida e dano base.
- `Item`: descreve armas, poções, chaves e tesouros coletáveis.
- `Room`: representa cada sala, suas conexões, itens e inimigos.
- `GameUI`: constrói a janela Swing com botões e área de log sincronizada com o console.

## Como compilar

No diretório raiz do projeto execute:

```bash
javac -d out $(find src -name "*.java")
```

O comando acima compila todas as classes para o diretório `out`.

## Como executar

Após a compilação, inicie o jogo com:

```bash
java -cp out aventura.Main
```

O terminal solicitará o nome do herói e apresentará o salão inicial. A janela Swing será
aberta automaticamente durante a execução.

> Dica: mantenha o console aberto para acompanhar as descrições completas e, se preferir,
> utilize os botões da janela para agilizar as escolhas.
