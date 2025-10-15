package aventura;

import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

/**
 * Represents a room inside the castle.
 */
public class Room {
    private final String id;
    private final String name;
    private final String description;
    private final Map<String, String> connections = new LinkedHashMap<>();
    private final List<Item> items = new ArrayList<>();
    private Enemy enemy;
    private boolean visited;

    public Room(String id, String name, String description) {
        this.id = id;
        this.name = name;
        this.description = description;
    }

    public String getId() {
        return id;
    }

    public String getName() {
        return name;
    }

    public String getDescription() {
        return description;
    }

    public void connect(String optionName, String roomId) {
        connections.put(optionName, roomId);
    }

    public Map<String, String> getConnections() {
        return new LinkedHashMap<>(connections);
    }

    public List<Item> getItems() {
        return new ArrayList<>(items);
    }

    public void addItem(Item item) {
        items.add(item);
    }

    public void removeItem(Item item) {
        items.remove(item);
    }

    public Enemy getEnemy() {
        return enemy;
    }

    public void setEnemy(Enemy enemy) {
        this.enemy = enemy;
    }

    public boolean isVisited() {
        return visited;
    }

    public void setVisited(boolean visited) {
        this.visited = visited;
    }
}
