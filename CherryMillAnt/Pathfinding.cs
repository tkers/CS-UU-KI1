﻿using System;
using System.Collections.Generic;
using Ants;

public static class Pathfinding
{
    public static Location FindNextLocation(Location start, Location dest, IGameState state)
    {
        return dest;
        List<Location> list = FindPath(start, dest, state);
        if (list != null)
            return list[0];
        else
            return null;
    }

    // Returns a list of tiles that form the shortest path between start and dest
    public static List<Location> FindPath(Location start, Location dest, IGameState state)
    {
        List<PathfindNode> open = new List<PathfindNode>();
        List<PathfindNode> closed = new List<PathfindNode>();

        // Starting node
        PathfindNode first = new PathfindNode(start);
        closed.Add(first);

        // Add all reachable tiles to the Open list.
        List<Location> reachable = GetNeighbours(first.Position, state);
        foreach (Location next in reachable)
        {
            if (state.GetIsPassable(next)) // Check if tile is free
                open.Add(new PathfindNode(next, first, dest));
        }

        // Repeat until the destination node is reached
        PathfindNode last = null;
        while (open.Count > 0)
        {

            // Search the best available tile (lowest cost to reach from start, closest to dest)
            PathfindNode best = null;
            foreach (PathfindNode next in open)
            {
                if (best == null)
                    best = next;

                if (next.F < best.F)
                    best = next;
            }

            // Move to closed list
            open.Remove(best);
            closed.Add(best);

            if (best.Position == dest) // Destination added to closed list - almost done!
            {
                last = best;
                break;
            }

            // Find tiles adjacent to this tile
            reachable = GetNeighbours(best.Position, state);
            foreach (Location next in reachable)
            {
                if (!state.GetIsPassable(next)) // Check if tile is blocked
                    continue;

                // Check if tile is not in closed list already
                bool cont = false;
                foreach (PathfindNode n in closed)
                {
                    if (n.Position == next)
                    {
                        cont = true;
                        break;
                    }
                }

                if (cont)
                    continue;

                // Check if tile is in open list already
                PathfindNode inOpen = null;
                foreach (PathfindNode n in open)
                {
                    if (n.Position == next)
                    {
                        inOpen = n;
                        break;
                    }
                }

                if (inOpen != null) // Update parent if this is a faster route
                {
                    if (best.G + 1 < inOpen.G)
                    {
                        inOpen.Parent = best;
                    }
                }
                else // Add tile to open list
                {
                    open.Add(new PathfindNode(next, best, dest));
                }
            }
        }

        // Trace the route from destination to start (using each node's parent property)
        List<PathfindNode> route = new List<PathfindNode>();
        while (last != first && last != null)
        {
            route.Add(last);
            last = last.Parent;
        }

        if (last != first)
            return null;

        // Reverse route and convert to Points
        List<Location> path = new List<Location>();
        for (int i = route.Count - 1; i >= 0; i--)
        {
            path.Add(route[i].Position);
        }

        // Return the list of Points
        return path;
    }

    static List<Location> GetNeighbours(Location loc, IGameState state)
    {
        List<Location> neighbours = new List<Location>();
        neighbours.Add(new Location((loc.Row + 1) % state.Height, loc.Col));
        neighbours.Add(new Location((loc.Row - 1 + state.Height) % state.Height, loc.Col));
        neighbours.Add(new Location(loc.Row, (loc.Col + 1) % state.Width));
        neighbours.Add(new Location(loc.Row, (loc.Col - 1 + state.Width) % state.Width));
        return neighbours;
    }
}

class PathfindNode
{
    public Location Position;
    public PathfindNode Parent;

    public int H; // Estimated cost to reach destination

    public int G // Cost to reach node from start
    {
        get
        {
            if (Parent == null)
                return 0;
            else
                return (Parent.G + 1);
        }
    }

    public int F // G + H
    {
        get
        {
            return (G + H);
        }
    }

    public PathfindNode(Location position)
    {
        this.Position = position;
    }

    public PathfindNode(Location position, PathfindNode parent, Location destination)
    {
        this.Position = position;
        this.Parent = parent;
        this.H = Math.Abs(position.Col - destination.Col) + Math.Abs(position.Row - destination.Row); // Estimate distance with Manhattan method
    }
}