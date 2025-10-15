package aventura;

import java.io.FileWriter;
import java.io.IOException;
import java.io.PrintWriter;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Random;
import java.util.Scanner;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;

/**
 * Controls the flow of "Aventura no Castelo Sombrio".
 */
public class Game implements Runnable {
    private final Player player;
    private final Map<String, Room> rooms;
    private Room currentRoom;
    private final Random random = new Random();
    private final BlockingQueue<String> actionQueue = new LinkedBlockingQueue<>();
    private final Scanner scanner;
    private volatile boolean running;
    private Thread consoleReaderThread;
    private GameUI ui;

    public Game(Player player, Scanner scanner) {
        this.player = player;
        this.scanner = scanner;
        this.rooms = createRooms();
        this.currentRoom = rooms.get("hall");
        if (currentRoom != null) {
            currentRoom.setVisited(true);
        }
    }

    private Map<String, Room> createRooms() {
        Map<String, Room> layout = new LinkedHashMap<>();
        Room hall = new Room("hall", "Salão Escuro",
                "Você desperta em um salão escuro iluminado por tochas vacilantes.");
        Room library = new Room("library", "Biblioteca Antiga",
                "Prateleiras empoeiradas cheias de tomos mágicos cercam o ambiente.");
        Room dungeon = new Room("dungeon", "Masmorra",
                "Correntes pendem das paredes e você ouve gemidos distantes.");
        Room tower = new Room("tower", "Torre do Feiticeiro",
                "Vitrais quebrados deixam o vento frio atravessar a sala." );
        Room armory = new Room("armory", "Arsenal Abandonado",
                "Armas enferrujadas estão espalhadas pelo chão.");
        Room gate = new Room("gate", "Portão Selado",
                "Um grande portão com runas brilhantes bloqueia a saída do castelo.");

        hall.connect("Seguir para a biblioteca", "library");
        hall.connect("Descer até a masmorra", "dungeon");
        hall.connect("Subir a torre", "tower");

        library.connect("Retornar ao salão", "hall");
        library.connect("Descer à masmorra", "dungeon");

        dungeon.connect("Subir ao salão", "hall");
        dungeon.connect("Explorar o arsenal", "armory");

        armory.connect("Voltar à masmorra", "dungeon");
        armory.connect("Subir à torre", "tower");

        tower.connect("Descer ao salão", "hall");
        tower.connect("Seguir ao portão", "gate");

        gate.connect("Retornar à torre", "tower");

        library.addItem(new Item("Poção Menor", Item.ItemType.POTION, 15,
                "Restaura 15 pontos de vida."));
        dungeon.addItem(new Item("Chave Rúnica", Item.ItemType.KEY, 0,
                "Abre o portão selado do castelo."));
        armory.addItem(new Item("Espada de Prata", Item.ItemType.WEAPON, 3,
                "Aumenta o dano de seus ataques."));
        tower.addItem(new Item("Poção Maior", Item.ItemType.POTION, 25,
                "Uma poção brilhante que cura muitas feridas."));
        gate.addItem(new Item("Gema Sombria", Item.ItemType.TREASURE, 0,
                "Um artefato antigo que pulsa energia."));

        library.setEnemy(new Enemy("Goblin Guardião", 18, 6));
        dungeon.setEnemy(new Enemy("Esqueleto", 20, 5));
        tower.setEnemy(new Enemy("Mago Sombrio", 28, 7));

        layout.put(hall.getId(), hall);
        layout.put(library.getId(), library);
        layout.put(dungeon.getId(), dungeon);
        layout.put(tower.getId(), tower);
        layout.put(armory.getId(), armory);
        layout.put(gate.getId(), gate);
        return layout;
    }

    public void setUi(GameUI ui) {
        this.ui = ui;
        if (ui != null) {
            ui.refreshState();
        }
    }

    @Override
    public void run() {
        start();
    }

    public void start() {
        if (running) {
            return;
        }
        running = true;
        startConsoleReader();
        log("Bem-vindo, " + player.getName() + "! Você está preso no Castelo Sombrio.");
        log("Use o console ou os botões da janela para jogar.");

        while (running && player.isAlive()) {
            showCurrentRoom();
            showMenu();
            String action = waitForAction();
            processAction(action);
            notifyUi();
        }

        if (!player.isAlive()) {
            log("Você tombou na escuridão. Fim da jornada.");
        }
        stop();
    }

    public void stop() {
        running = false;
        if (consoleReaderThread != null) {
            consoleReaderThread.interrupt();
        }
        notifyUi();
    }

    private void startConsoleReader() {
        consoleReaderThread = new Thread(() -> {
            while (running) {
                try {
                    if (!scanner.hasNextLine()) {
                        break;
                    }
                    String input = scanner.nextLine();
                    submitAction(input);
                } catch (Exception e) {
                    break;
                }
            }
        }, "console-reader");
        consoleReaderThread.setDaemon(true);
        consoleReaderThread.start();
    }

    private void showCurrentRoom() {
        log("\n=== " + currentRoom.getName() + " ===");
        if (!currentRoom.isVisited()) {
            currentRoom.setVisited(true);
        }
        log(currentRoom.getDescription());
        Enemy enemy = currentRoom.getEnemy();
        if (enemy != null && enemy.isAlive()) {
            log("Um " + enemy.getType() + " bloqueia seu caminho!" );
        }
        List<Item> items = currentRoom.getItems();
        if (!items.isEmpty()) {
            log("Itens visíveis: ");
            for (Item item : items) {
                log(" - " + item);
            }
        }
    }

    private void showMenu() {
        log("\nEscolha sua ação:");
        log("1) Explorar outra sala");
        log("2) Confrontar o inimigo");
        log("3) Coletar itens da sala");
        log("4) Ver inventário");
        log("5) Usar item");
        log("6) Salvar progresso");
        log("7) Tentar escapar do castelo");
        log("8) Encerrar aventura");
    }

    private String waitForAction() {
        try {
            return actionQueue.take();
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            return "";
        }
    }

    public void submitAction(String action) {
        if (action == null) {
            return;
        }
        try {
            actionQueue.put(action.trim());
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }
    }

    private void processAction(String rawAction) {
        if (rawAction == null) {
            return;
        }
        String action = rawAction.trim();
        String normalized = action.toLowerCase();
        if (normalized.startsWith("explore:")) {
            handleExploreCommand(action.substring(8));
            return;
        }
        if (normalized.startsWith("use:")) {
            handleUseCommand(action.substring(4));
            return;
        }

        switch (normalized) {
            case "1":
            case "explorar":
                handleExplorePrompt();
                break;
            case "2":
            case "lutar":
                handleFight();
                break;
            case "3":
            case "coletar":
                collectItems();
                break;
            case "4":
            case "inventario":
            case "inventário":
                showInventory();
                break;
            case "5":
            case "usar":
                handleUsePrompt();
                break;
            case "6":
            case "salvar":
                saveProgress();
                break;
            case "7":
            case "escapar":
                attemptEscape();
                break;
            case "8":
            case "sair":
                log("Você decide abandonar a aventura por enquanto.");
                running = false;
                break;
            default:
                log("Não entendi a ação '" + action + "'. Tente novamente.");
        }
    }

    private void handleExploreCommand(String roomId) {
        Room nextRoom = rooms.get(roomId.trim());
        if (nextRoom == null) {
            log("Não há caminho para " + roomId + ".");
            return;
        }
        moveToRoom(nextRoom);
    }

    private void handleExplorePrompt() {
        Map<String, String> connections = currentRoom.getConnections();
        if (connections.isEmpty()) {
            log("Não há saídas imediatas desta sala.");
            return;
        }
        List<Map.Entry<String, String>> options = new ArrayList<>(connections.entrySet());
        log("Para onde deseja ir?");
        for (int i = 0; i < options.size(); i++) {
            log((i + 1) + ") " + options.get(i).getKey());
        }
        while (true) {
            String choice = waitForAction();
            if (choice == null) {
                return;
            }
            String trimmed = choice.trim();
            if (trimmed.toLowerCase().startsWith("explore:")) {
                handleExploreCommand(trimmed.substring(8));
                return;
            }
            try {
                int option = Integer.parseInt(trimmed);
                if (option >= 1 && option <= options.size()) {
                    moveToRoom(rooms.get(options.get(option - 1).getValue()));
                    return;
                }
            } catch (NumberFormatException ex) {
                // ignore
            }
            log("Escolha inválida. Informe o número da sala desejada.");
        }
    }

    private void moveToRoom(Room nextRoom) {
        if (nextRoom == null) {
            return;
        }
        currentRoom = nextRoom;
        currentRoom.setVisited(true);
        log("Você segue para " + currentRoom.getName() + ".");
    }

    private void handleFight() {
        Enemy enemy = currentRoom.getEnemy();
        if (enemy == null || !enemy.isAlive()) {
            log("Não há inimigos para lutar aqui.");
            return;
        }
        log("Você enfrenta o " + enemy.getType() + "!" );
        while (enemy.isAlive() && player.isAlive()) {
            int playerVariance = random.nextInt(4); // 0-3 extra damage
            int damage = player.getAttackDamage(playerVariance);
            enemy.takeDamage(damage);
            log("Você causa " + damage + " de dano. (Vida inimigo: " + enemy.getHealth() + ")");
            if (!enemy.isAlive()) {
                log("O inimigo cai derrotado!");
                break;
            }
            int enemyDamage = enemy.attack();
            player.takeDamage(enemyDamage);
            log("O " + enemy.getType() + " atinge você por " + enemyDamage + " de dano. (Sua vida: "
                    + player.getHealth() + ")");
            if (!player.isAlive()) {
                break;
            }
        }
        if (!enemy.isAlive()) {
            currentRoom.setEnemy(null);
        }
        if (!player.isAlive()) {
            running = false;
        }
    }

    private void collectItems() {
        List<Item> roomItems = new ArrayList<>(currentRoom.getItems());
        if (roomItems.isEmpty()) {
            log("Não há itens para coletar.");
            return;
        }
        for (Item item : roomItems) {
            currentRoom.removeItem(item);
            player.addItem(item);
            log("Você coleta: " + item.getName());
            if (item.getType() == Item.ItemType.WEAPON) {
                player.equipWeapon(item);
                log("Você empunha " + item.getName() + ". Seus ataques ficarão mais fortes!");
            }
        }
    }

    private void showInventory() {
        List<Item> inventory = player.getInventory();
        if (inventory.isEmpty()) {
            log("Seu inventário está vazio.");
            return;
        }
        log("Inventário de " + player.getName() + ":");
        int index = 1;
        for (Item item : inventory) {
            log(index + ") " + item);
            index++;
        }
    }

    private void handleUsePrompt() {
        List<Item> usable = getUsableItems();
        if (usable.isEmpty()) {
            log("Você não possui itens utilizáveis agora.");
            return;
        }
        log("Qual item deseja usar?");
        for (int i = 0; i < usable.size(); i++) {
            log((i + 1) + ") " + usable.get(i));
        }
        while (true) {
            String choice = waitForAction();
            if (choice == null) {
                return;
            }
            String trimmed = choice.trim();
            if (trimmed.toLowerCase().startsWith("use:")) {
                handleUseCommand(trimmed.substring(4));
                return;
            }
            try {
                int option = Integer.parseInt(trimmed);
                if (option >= 1 && option <= usable.size()) {
                    useItem(usable.get(option - 1));
                    return;
                }
            } catch (NumberFormatException ex) {
                // ignore
            }
            log("Escolha inválida. Informe o número do item.");
        }
    }

    private void handleUseCommand(String command) {
        try {
            int index = Integer.parseInt(command.trim());
            List<Item> usable = getUsableItems();
            if (index >= 1 && index <= usable.size()) {
                useItem(usable.get(index - 1));
            } else {
                log("Não foi possível usar o item informado.");
            }
        } catch (NumberFormatException ex) {
            log("Comando de uso inválido.");
        }
    }

    private void useItem(Item item) {
        if (item.getType() == Item.ItemType.POTION) {
            player.heal(item.getPower());
            player.removeItem(item);
            log("Você bebe " + item.getName() + " e recupera " + item.getPower()
                    + " de vida. (Vida atual: " + player.getHealth() + ")");
        } else if (item.getType() == Item.ItemType.WEAPON) {
            player.equipWeapon(item);
            log("Você reforça seu ataque com " + item.getName() + ".");
        } else {
            log("Não é possível usar este item agora.");
        }
    }

    private List<Item> getUsableItems() {
        List<Item> usable = new ArrayList<>();
        for (Item item : player.getInventory()) {
            if (item.getType() == Item.ItemType.POTION || item.getType() == Item.ItemType.WEAPON) {
                usable.add(item);
            }
        }
        return usable;
    }

    private void saveProgress() {
        try (PrintWriter writer = new PrintWriter(new FileWriter("savegame.txt", false))) {
            writer.println("Aventura no Castelo Sombrio");
            writer.println("Salvo em: " + LocalDateTime.now().format(DateTimeFormatter.ISO_LOCAL_DATE_TIME));
            writer.println("Jogador: " + player.getName());
            writer.println("Vida: " + player.getHealth() + "/" + player.getMaxHealth());
            writer.println("Sala atual: " + currentRoom.getName());
            writer.println("Inventário:");
            for (Item item : player.getInventory()) {
                writer.println(" - " + item);
            }
            log("Progresso salvo em savegame.txt.");
        } catch (IOException e) {
            log("Não foi possível salvar o jogo: " + e.getMessage());
        }
    }

    private void attemptEscape() {
        if (!"gate".equals(currentRoom.getId())) {
            log("Você precisa encontrar o portão selado antes de escapar.");
            return;
        }
        if (!player.hasKey()) {
            log("As runas brilham intensamente. Parece que falta uma chave especial.");
            return;
        }
        log("Você usa a chave rúnica e o portão se abre lentamente! Você escapa do Castelo Sombrio.");
        running = false;
    }

    private void notifyUi() {
        if (ui != null) {
            ui.refreshState();
        }
    }

    private void log(String message) {
        System.out.println(message);
        if (ui != null) {
            ui.appendMessage(message);
        }
    }

    public boolean hasActiveEnemy() {
        Enemy enemy = currentRoom.getEnemy();
        return enemy != null && enemy.isAlive();
    }

    public boolean hasCollectableItems() {
        return !currentRoom.getItems().isEmpty();
    }

    public Map<String, String> getAvailableConnections() {
        return currentRoom.getConnections();
    }

    public List<Item> getInventorySnapshot() {
        return new ArrayList<>(player.getInventory());
    }

    public List<Item> getUsableItemsSnapshot() {
        return new ArrayList<>(getUsableItems());
    }

    public boolean canAttemptEscape() {
        return "gate".equals(currentRoom.getId());
    }

    public Player getPlayer() {
        return player;
    }

    public Room getCurrentRoom() {
        return currentRoom;
    }
}
