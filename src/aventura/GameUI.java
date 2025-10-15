package aventura;

import javax.swing.JButton;
import javax.swing.JFrame;
import javax.swing.JOptionPane;
import javax.swing.JPanel;
import javax.swing.JScrollPane;
import javax.swing.JTextArea;
import javax.swing.SwingUtilities;
import javax.swing.WindowConstants;
import java.awt.BorderLayout;
import java.awt.Dimension;
import java.awt.GridLayout;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/**
 * Simple Swing interface to play the text adventure using buttons.
 */
public class GameUI extends JFrame {
    private final transient Game game;
    private final JTextArea logArea = new JTextArea();
    private final JButton fightButton = new JButton("Lutar");
    private final JButton collectButton = new JButton("Coletar");
    private final JButton escapeButton = new JButton("Escapar");

    public GameUI(Game game) {
        super("Aventura no Castelo Sombrio");
        this.game = game;
        configureComponents();
        configureActions();
        game.setUi(this);
    }

    private void configureComponents() {
        setDefaultCloseOperation(WindowConstants.EXIT_ON_CLOSE);
        setLayout(new BorderLayout());

        logArea.setEditable(false);
        logArea.setLineWrap(true);
        logArea.setWrapStyleWord(true);
        JScrollPane scrollPane = new JScrollPane(logArea);
        scrollPane.setPreferredSize(new Dimension(520, 360));
        add(scrollPane, BorderLayout.CENTER);

        JPanel buttonsPanel = new JPanel(new GridLayout(2, 4, 8, 8));

        JButton exploreButton = new JButton("Explorar");
        buttonsPanel.add(exploreButton);

        buttonsPanel.add(fightButton);

        buttonsPanel.add(collectButton);

        JButton inventoryButton = new JButton("Inventário");
        buttonsPanel.add(inventoryButton);

        JButton useButton = new JButton("Usar Item");
        buttonsPanel.add(useButton);

        JButton saveButton = new JButton("Salvar");
        buttonsPanel.add(saveButton);

        buttonsPanel.add(escapeButton);

        JButton quitButton = new JButton("Encerrar");
        buttonsPanel.add(quitButton);

        add(buttonsPanel, BorderLayout.SOUTH);

        pack();
        setLocationRelativeTo(null);
        setVisible(true);

        exploreButton.addActionListener(e -> handleExploreAction());
        fightButton.addActionListener(e -> game.submitAction("2"));
        collectButton.addActionListener(e -> game.submitAction("3"));
        inventoryButton.addActionListener(e -> game.submitAction("4"));
        useButton.addActionListener(e -> handleUseAction());
        saveButton.addActionListener(e -> game.submitAction("6"));
        escapeButton.addActionListener(e -> game.submitAction("7"));
        quitButton.addActionListener(e -> game.submitAction("8"));
    }

    private void configureActions() {
        // Placeholder to emphasize separation of concerns in case of future expansion.
    }

    private void handleExploreAction() {
        Map<String, String> connections = game.getAvailableConnections();
        if (connections.isEmpty()) {
            appendMessage("Não há rotas disponíveis a partir daqui.");
            return;
        }
        List<String> labels = new ArrayList<>();
        List<String> ids = new ArrayList<>();
        for (Map.Entry<String, String> entry : connections.entrySet()) {
            labels.add(entry.getKey());
            ids.add(entry.getValue());
        }
        Object choice = JOptionPane.showInputDialog(this, "Selecione o destino:",
                "Explorar", JOptionPane.PLAIN_MESSAGE, null,
                labels.toArray(), labels.get(0));
        if (choice != null) {
            int index = labels.indexOf(choice.toString());
            if (index >= 0) {
                game.submitAction("explore:" + ids.get(index));
            }
        }
    }

    private void handleUseAction() {
        List<Item> usable = game.getUsableItemsSnapshot();
        if (usable.isEmpty()) {
            appendMessage("Nenhum item pode ser usado agora.");
            return;
        }
        List<String> labels = new ArrayList<>();
        for (int i = 0; i < usable.size(); i++) {
            labels.add((i + 1) + ". " + usable.get(i));
        }
        Object choice = JOptionPane.showInputDialog(this, "Escolha um item para usar:",
                "Itens", JOptionPane.PLAIN_MESSAGE, null,
                labels.toArray(), labels.get(0));
        if (choice != null) {
            int index = labels.indexOf(choice.toString());
            if (index >= 0) {
                game.submitAction("use:" + (index + 1));
            }
        }
    }

    public void appendMessage(String message) {
        SwingUtilities.invokeLater(() -> {
            logArea.append(message + "\n");
            logArea.setCaretPosition(logArea.getDocument().getLength());
        });
    }

    public void refreshState() {
        SwingUtilities.invokeLater(() -> {
            fightButton.setEnabled(game.hasActiveEnemy());
            collectButton.setEnabled(game.hasCollectableItems());
            escapeButton.setEnabled(game.canAttemptEscape());
            setTitle(String.format("Aventura no Castelo Sombrio - %s (%d/%d de vida) - %s",
                    game.getPlayer().getName(),
                    game.getPlayer().getHealth(),
                    game.getPlayer().getMaxHealth(),
                    game.getCurrentRoom().getName()));
        });
    }
}
