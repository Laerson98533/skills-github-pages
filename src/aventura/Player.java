package aventura;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * Represents the hero controlled by the player.
 */
public class Player {
    private final String name;
    private final int maxHealth;
    private int health;
    private final List<Item> inventory = new ArrayList<>();
    private final int baseDamage = 6;
    private int weaponBonus = 0;

    public Player(String name, int maxHealth) {
        this.name = name;
        this.maxHealth = maxHealth;
        this.health = maxHealth;
    }

    public String getName() {
        return name;
    }

    public int getHealth() {
        return health;
    }

    public int getMaxHealth() {
        return maxHealth;
    }

    public List<Item> getInventory() {
        return Collections.unmodifiableList(inventory);
    }

    public void addItem(Item item) {
        inventory.add(item);
    }

    public void removeItem(Item item) {
        inventory.remove(item);
    }

    public boolean hasItem(String itemName) {
        return inventory.stream().anyMatch(item -> item.getName().equalsIgnoreCase(itemName));
    }

    public void takeDamage(int amount) {
        health = Math.max(0, health - amount);
    }

    public void heal(int amount) {
        health = Math.min(maxHealth, health + amount);
    }

    public boolean isAlive() {
        return health > 0;
    }

    public void equipWeapon(Item weapon) {
        if (weapon.getType() == Item.ItemType.WEAPON) {
            weaponBonus = Math.max(weaponBonus, weapon.getPower());
        }
    }

    public int getAttackDamage(int variance) {
        int bonus = variance >= 0 ? variance : 0;
        return baseDamage + weaponBonus + bonus;
    }

    public boolean hasKey() {
        return inventory.stream().anyMatch(item -> item.getType() == Item.ItemType.KEY);
    }

    @Override
    public String toString() {
        return name + " - Vida: " + health + "/" + maxHealth;
    }
}
