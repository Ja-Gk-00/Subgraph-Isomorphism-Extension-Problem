# Subgraph-Isomorphism-Extension-Problem
To be added later

## And what is `SIEP`?
**S**ubgraph **I**somorphism **E**xtension **P**roblem

## How to build & run
In the root dir of this repo do:
```bash
dotnet restore
dotnet build
```

To run:
```bash
./src/SIEP/bin/Debug/net9.0/siep -f samples/ok_1
```

## Extending to multigraphs
```
// P - pattern, T - target
// All is the same, adj matrix has ints (for now, maybe sth else for optimization)

// Init candadates (could be split for out/in degrees)
if (deg(T, j) >= deg(P, i) && no_of_loops(T, j) >= no_of_loops(P, i)) {
    M_candidate[i][j] = 1
}

// Pruning - change the check to
if (no_of_edges_between(T, v, v') < no_of_edges_between(P, u, u')) {
    M_candidate[i][j] = 0
}

// I dont have more :(. We will see if it's enough

```

## TODO
Delete samples