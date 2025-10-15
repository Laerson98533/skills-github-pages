package aventura;

import java.util.Random;

/**
 * Represents an enemy inside the castle.
 */
public class Enemy {
    private final String type;
    private int health;
    private final int baseDamage;
    private final Random random = new Random();

    public Enemy(String type, int health, int baseDamage) {
        this.type = type;
        this.health = health;
        this.baseDamage = baseDamage;
    }

    public String getType() {
        return type;
    }

    public int getHealth() {
        return health;
    }

    public boolean isAlive() {
        return health > 0;
    }

    public void takeDamage(int amount) {
        health = Math.max(0, health - amount);
    }

    public int attack() {
        int variance = random.nextInt(5); // 0-4 bonus damage
        return Math.max(1, baseDamage + variance - 2);
    }

    @Override
    public String toString() {
        return type + " (Vida: " + health + ")";
    }
}
