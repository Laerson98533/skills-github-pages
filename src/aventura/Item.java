package aventura;

/**
 * Represents an item that can be collected or used by the player.
 */
public class Item {
    /** Possible item categories. */
    public enum ItemType {
        WEAPON,
        POTION,
        KEY,
        TREASURE
    }

    private final String name;
    private final ItemType type;
    private final int power;
    private final String description;

    public Item(String name, ItemType type, int power, String description) {
        this.name = name;
        this.type = type;
        this.power = power;
        this.description = description;
    }

    public String getName() {
        return name;
    }

    public ItemType getType() {
        return type;
    }

    public int getPower() {
        return power;
    }

    public String getDescription() {
        return description;
    }

    @Override
    public String toString() {
        return name + " (" + type + ") - " + description;
    }
}
