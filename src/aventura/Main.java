package aventura;

import javax.swing.SwingUtilities;
import java.awt.GraphicsEnvironment;
import java.util.Scanner;

/**
 * Application entry point.
 */
public class Main {
    public static void main(String[] args) {
        Scanner scanner = new Scanner(System.in);
        System.out.println("==============================");
        System.out.println(" Aventura no Castelo Sombrio ");
        System.out.println("==============================");
        System.out.print("Digite o nome do herói: ");
        String name = scanner.nextLine().trim();
        if (name.isEmpty()) {
            name = "Herói";
        }

        Player player = new Player(name, 60);
        Game game = new Game(player, scanner);

        if (GraphicsEnvironment.isHeadless()) {
            System.out.println("Ambiente sem suporte gráfico detectado. Continuando apenas no console.");
        } else {
            SwingUtilities.invokeLater(() -> new GameUI(game));
        }

        game.start();
    }
}
