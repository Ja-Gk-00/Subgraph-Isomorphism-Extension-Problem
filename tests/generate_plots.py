import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from scipy.special import factorial
import os

# --- CONFIGURATION ---
RESULTS_DIR = 'results'
PLOTS_DIR = 'plots'

# Create plots directory if it doesn't exist
os.makedirs(PLOTS_DIR, exist_ok=True)

# Set plot style
plt.style.use('ggplot')
plt.rcParams['font.size'] = 12
plt.rcParams['figure.figsize'] = (10, 6)
plt.rcParams['axes.grid'] = True

def load_data(filename):
    """
    Helper function to load CSV data safely.
    """
    path = os.path.join(RESULTS_DIR, filename)
    if not os.path.exists(path):
        print(f"[WARNING] File {path} not found. Skipping this plot.")
        return None
    try:
        return pd.read_csv(path)
    except Exception as e:
        print(f"[ERROR] Reading {filename}: {e}")
        return None

def calculate_graph_size(df):
    """
    Adds a 'Graph_Size' column = Size_N (|V|) + Est_Edges (|E|)
    """
    if 'Est_Edges' not in df.columns:
        # Default density assumption 0.4 if missing
        df['Est_Edges'] = (0.4 * df['Size_N'] * (df['Size_N'] - 1) / 2).astype(int)
    
    df['Graph_Size'] = df['Size_N'] + df['Est_Edges']
    return df

# ---------------------------------------------------------
# 1. METRIC LINEARITY (Time vs |V|+|E|) + Theoretical Fit
# ---------------------------------------------------------
def plot_metric_linearity():
    print("[INFO] Generating Metric Linearity plot...")
    df = load_data('metric.csv')
    if df is None: return
    df = calculate_graph_size(df)

    plt.figure()
    
    x = df['Graph_Size']
    y = df['Avg_Time_ms']
    plt.plot(x, y, marker='o', linestyle='-', color='b', label='Pomiary (WL Kernel)')
    
    # Linear Fit
    slope, intercept = np.polyfit(x, y, 1)
    x_theo = np.linspace(x.min(), x.max(), 100)
    y_theo = slope * x_theo + intercept
    
    plt.plot(x_theo, y_theo, color='black', linestyle='--', linewidth=2, 
             label=f'Teoria O(|V|+|E|) [Fit]')
    
    plt.title('Złożoność Metryki WL Kernel')
    plt.xlabel('Rozmiar Grafu (|V| + |E|)')
    plt.ylabel('Czas wykonywania [ms]')
    plt.legend()
    
    output_path = os.path.join(PLOTS_DIR, '01_metric_linearity.png')
    plt.savefig(output_path)
    print(f"   Saved to: {output_path}")
    plt.close()

# ---------------------------------------------------------
# 2. METRIC MULTIGRAPH STABILITY
# ---------------------------------------------------------
def plot_metric_multigraph():
    print("[INFO] Generating Multigraph Metric plot...")
    df = load_data('multi_metric.csv')
    if df is None: return
    df = calculate_graph_size(df)

    plt.figure()
    
    weights = df['Max_Weight'].unique()
    
    for w in sorted(weights):
        subset = df[df['Max_Weight'] == w]
        subset = subset.sort_values(by='Graph_Size')
        
        label = f'Multigraf (Waga max: {w})' if w > 1 else 'Graf Prosty (Waga: 1)'
        plt.plot(subset['Graph_Size'], subset['Avg_Time_ms'], marker='s', linestyle='--', label=label)

    plt.title('Wpływ krotności krawędzi na czas metryki')
    plt.xlabel('Rozmiar Grafu (|V| + |E|)')
    plt.ylabel('Czas wykonywania [ms]')
    plt.legend()
    
    output_path = os.path.join(PLOTS_DIR, '02_metric_multigraph.png')
    plt.savefig(output_path)
    print(f"   Saved to: {output_path}")
    plt.close()

# ---------------------------------------------------------
# 3. EXACT ISOMORPHISM (Exponential) + Optimized Fit
# ---------------------------------------------------------
def plot_isomorphism_exact():
    print("[INFO] Generating Exact Isomorphism plot...")
    df = load_data('isomorphism.csv')
    if df is None: return
    df = calculate_graph_size(df)

    plt.figure()
    
    algos = df['Algorithm'].unique()
    scaling_factors = []

    for algo in algos:
        subset = df[df['Algorithm'] == algo].sort_values(by='Size_N')
        # Use simple Size_N for X-axis as N! depends on N, not edges
        subset = subset[subset['Avg_Time_ms'] > 0]
        
        if not subset.empty:
            plt.plot(subset['Size_N'], subset['Avg_Time_ms'], marker='o', label=f'Algorytm {algo}')
            
            # Calculate K for fit
            last_point = subset.iloc[-1]
            try:
                n_val = int(last_point['Size_N'])
                t_val = last_point['Avg_Time_ms']
                fact_val = float(factorial(n_val))
                k = t_val / fact_val
                scaling_factors.append(k)
            except:
                pass

    # Theoretical Fit (Geometric Mean)
    if scaling_factors:
        try:
            k_avg = np.exp(np.mean(np.log(scaling_factors)))
            x_min = df['Size_N'].min()
            x_max = df['Size_N'].max()
            x_theo = np.arange(x_min, x_max + 1)
            y_theo = k_avg * factorial(x_theo.astype(int))
            
            plt.plot(x_theo, y_theo, color='black', linestyle=':', linewidth=2, 
                     label='Teoria O(N!) [Średni Fit]')
        except OverflowError:
            print("[WARNING] Factorial too large.")

    plt.title('Złożoność Algorytmów Dokładnych (Izomorfizm)')
    plt.xlabel('Liczba wierzchołków (N)')
    plt.ylabel('Czas wykonywania [ms] (Skala Log)')
    plt.yscale('log')
    plt.legend()
    
    output_path = os.path.join(PLOTS_DIR, '03_isomorphism_exact.png')
    plt.savefig(output_path)
    print(f"   Saved to: {output_path}")
    plt.close()

# ---------------------------------------------------------
# 4. EXTENSION QUALITY (Bar Chart)
# ---------------------------------------------------------
def plot_extension_quality():
    print("[INFO] Generating Extension Quality plot...")
    df = load_data('quality_extension.csv')
    if df is None: return

    pivot_v = df.pivot(index='Case', columns='Algorithm', values='Added_Vertices')
    
    ax = pivot_v.plot(kind='bar', rot=0)
    
    plt.title('Jakość Rozszerzania: Liczba Dodanych Wierzchołków')
    plt.xlabel('Instancja testowa (Ziarno)')
    plt.ylabel('Liczba dodanych wierzchołków')
    plt.legend(title='Strategia')
    
    plt.tight_layout()
    output_path = os.path.join(PLOTS_DIR, '04_extension_quality_vertices.png')
    plt.savefig(output_path)
    print(f"   Saved to: {output_path}")
    plt.close()

# ---------------------------------------------------------
# 5. EXACT EXTENSION MULTIGRAPH
# ---------------------------------------------------------
def plot_extension_multigraph_exact():
    print("[INFO] Generating Exact Multigraph Extension plot...")
    df = load_data('extension_multigraph_exact.csv')
    if df is None: return
    df = calculate_graph_size(df)

    plt.figure()
    
    df['Label'] = df['Algorithm'].str.replace('+Ullmann', '', regex=False)
    
    labels = df['Label'].unique()
    for lbl in labels:
        subset = df[df['Label'] == lbl].sort_values(by='Graph_Size')
        plt.plot(subset['Graph_Size'], subset['Avg_Time_ms'], marker='x', label=lbl)

    plt.title('Algorytmy Dokładne na Multigrafach (Extension)')
    plt.xlabel('Rozmiar Grafu (|V| + |E|)')
    plt.ylabel('Czas wykonywania [ms]')
    plt.legend()
    
    output_path = os.path.join(PLOTS_DIR, '05_extension_multigraph_exact.png')
    plt.savefig(output_path)
    print(f"   Saved to: {output_path}")
    plt.close()

# ---------------------------------------------------------
# 6. SUBGRAPH ISOMORPHISM (Fixed Pattern) - NEW!
# ---------------------------------------------------------
def plot_subgraph_fixed_pattern():
    print("[INFO] Generating Subgraph Fixed Pattern plot...")
    df = load_data('subgraph_fixed_pattern.csv')
    if df is None: return

    plt.figure()
    
    # We retrieve the fixed Pattern N1 to include in the label
    n1_val = df['Pattern_N1'].iloc[0]

    # Plot X=Target_N2, Y=Time
    plt.plot(df['Target_N2'], df['Avg_Time_ms'], marker='D', linestyle='-', color='purple', 
             label=f'Ullmann (Wzorzec N1={n1_val})')

    plt.title(f'Izomorfizm Podgrafu: Stały wzorzec w rosnącym tle')
    plt.xlabel('Rozmiar Grafu Docelowego (N2)')
    plt.ylabel('Czas wykonywania [ms]')
    plt.legend()
    plt.grid(True)
    
    output_path = os.path.join(PLOTS_DIR, '06_subgraph_fixed_pattern.png')
    plt.savefig(output_path)
    print(f"   Saved to: {output_path}")
    plt.close()

# --- MAIN EXECUTION ---
if __name__ == "__main__":
    print(f"Starting plot generation reading from '{RESULTS_DIR}'...")
    
    try:
        plot_metric_linearity()
        plot_metric_multigraph()
        plot_isomorphism_exact()
        plot_extension_quality()
        plot_extension_multigraph_exact()
        plot_subgraph_fixed_pattern()
    except Exception as e:
        print(f"[ERROR] An unexpected error occurred: {e}")
    
    print(f"\nAll done! Check the '{PLOTS_DIR}' folder.")