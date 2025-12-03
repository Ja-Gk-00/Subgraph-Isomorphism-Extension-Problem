import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from scipy.special import factorial
import os

# --- CONFIGURATION ---
RESULTS_DIR = 'results'
PLOTS_DIR = 'plots'

os.makedirs(PLOTS_DIR, exist_ok=True)

plt.style.use('ggplot')
plt.rcParams['font.size'] = 12
plt.rcParams['figure.figsize'] = (10, 6)
plt.rcParams['axes.grid'] = True

def load_data(filename):
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
    if 'Est_Edges' not in df.columns:
        df['Est_Edges'] = (0.6 * df['Size_N'] * (df['Size_N'] - 1) / 2).astype(int)
    
    df['Graph_Size'] = df['Size_N'] + df['Est_Edges']
    return df

def add_secondary_xaxis(ax, df):
    mapping = df[['Graph_Size', 'Size_N']].drop_duplicates().sort_values('Graph_Size')
    
    if len(mapping) > 5:
        selected_indices = [0, len(mapping) - 1]
        
        last_pos = mapping.iloc[0]['Graph_Size']
        total_span = mapping.iloc[-1]['Graph_Size'] - mapping.iloc[0]['Graph_Size']
        min_gap = total_span * 0.15
        
        for i in range(1, len(mapping) - 1):
            current_pos = mapping.iloc[i]['Graph_Size']
            if current_pos - last_pos > min_gap:
                selected_indices.append(i)
                last_pos = current_pos
                
        selected_indices = sorted(list(set(selected_indices)))
        mapping = mapping.iloc[selected_indices]

    ax2 = ax.twiny()
    ax2.set_xlim(ax.get_xlim())
    ax2.set_xticks(mapping['Graph_Size'].values)
    ax2.set_xticklabels(mapping['Size_N'].values.astype(str))
    ax2.set_xlabel('Liczba Wierzchołków (N)')
    ax2.grid(False)

def add_secondary_xaxis_by_N(ax, df):
    # Adds a secondary top X-axis showing 'Graph_Size' (|V|+|E|)
    # corresponding to the 'N' on the bottom X-axis.
    mapping = df[['Size_N', 'Graph_Size']].drop_duplicates().sort_values('Size_N')

    # Smart filtering to prevent label overlap.
    if len(mapping) > 5:
        selected_indices = [0, len(mapping) - 1]
        
        last_pos = mapping.iloc[0]['Size_N']
        total_span = mapping.iloc[-1]['Size_N'] - mapping.iloc[0]['Size_N']
        min_gap = total_span * 0.15
        
        for i in range(1, len(mapping) - 1):
            current_pos = mapping.iloc[i]['Size_N']
            if current_pos - last_pos > min_gap:
                selected_indices.append(i)
                last_pos = current_pos
                
        selected_indices = sorted(list(set(selected_indices)))
        mapping = mapping.iloc[selected_indices]

    ax2 = ax.twiny()
    ax2.set_xlim(ax.get_xlim())
    
    labels = []
    for size in mapping['Graph_Size'].values:
        if size >= 1000:
            labels.append(f'{size/1000:.1f}k')
        else:
            labels.append(str(size))

    ax2.set_xticks(mapping['Size_N'].values)
    ax2.set_xticklabels(labels)
    ax2.set_xlabel('Rozmiar Grafu (|V| + |E|)')
    ax2.grid(False)

# ---------------------------------------------------------
# 1. METRIC LINEARITY
# ---------------------------------------------------------
def plot_metric_linearity():
    print("[INFO] Generating Metric Linearity plot...")
    df = load_data('metric.csv')
    if df is None: return
    df = calculate_graph_size(df)

    fig, ax = plt.subplots()
    
    x = df['Graph_Size']
    y = df['Avg_Time_ms']
    ax.plot(x, y, marker='o', linestyle='-', color='b', label='Pomiary (WL Kernel)')
    
    slope, intercept = np.polyfit(x, y, 1)
    x_theo = np.linspace(x.min(), x.max(), 100)
    y_theo = slope * x_theo + intercept
    
    ax.plot(x_theo, y_theo, color='black', linestyle='--', linewidth=2, 
             label=f'Teoria O(|V|+|E|) [Fit]')
    
    ax.set_title('Złożoność Metryki WL Kernel')
    ax.set_xlabel('Rozmiar Grafu (|V| + |E|)')
    ax.set_ylabel('Czas wykonywania [ms]')
    ax.legend()
    
    add_secondary_xaxis(ax, df)
    
    plt.tight_layout()
    plt.savefig(os.path.join(PLOTS_DIR, '01_metric_linearity.png'))
    plt.close()

# ---------------------------------------------------------
# 2. METRIC MULTIGRAPH
# ---------------------------------------------------------
def plot_metric_multigraph():
    print("[INFO] Generating Multigraph Metric plot...")
    df = load_data('multi_metric.csv')
    if df is None: return
    df = calculate_graph_size(df)

    # --- Plot 1: X-axis = |V|+|E| ---
    fig, ax = plt.subplots()
    
    weights = df['Max_Weight'].unique()
    for w in sorted(weights):
        subset = df[df['Max_Weight'] == w].sort_values(by='Graph_Size')
        label = f'Multigraf (Waga max: {w})' if w > 1 else 'Graf Prosty (Waga: 1)'
        ax.plot(subset['Graph_Size'], subset['Avg_Time_ms'], marker='s', linestyle='--', label=label)

    ax.set_title('Wpływ krotności krawędzi na czas metryki')
    ax.set_xlabel('Rozmiar Grafu (|V| + |E|)')
    ax.set_ylabel('Czas wykonywania [ms]')
    ax.legend()
    
    add_secondary_xaxis(ax, df)
    
    plt.tight_layout()
    plt.savefig(os.path.join(PLOTS_DIR, '02_metric_multigraph.png'))
    plt.close()

    # --- Plot 2: X-axis = N ---
    fig, ax = plt.subplots()
    
    weights = df['Max_Weight'].unique()
    for w in sorted(weights):
        subset = df[df['Max_Weight'] == w].sort_values(by='Size_N')
        label = f'Multigraf (Waga max: {w})' if w > 1 else 'Graf Prosty (Waga: 1)'
        ax.plot(subset['Size_N'], subset['Avg_Time_ms'], marker='s', linestyle='--', label=label, alpha=0.8)

    x_all = df['Size_N']
    y_all = df['Avg_Time_ms']
    if len(x_all) > 1:
        coeffs = np.polyfit(x_all, y_all, 2)
        poly_eqn = np.poly1d(coeffs)
        x_theo = np.linspace(x_all.min(), x_all.max(), 100)
        y_theo = poly_eqn(x_theo)
        ax.plot(x_theo, y_theo, color='black', linestyle=':', linewidth=2, label='Fit O(N^2)')

    ax.set_title('Wpływ krotności krawędzi na czas metryki (wg N)')
    ax.set_xlabel('Liczba Wierzchołków (N)')
    ax.set_ylabel('Czas wykonywania [ms]')
    ax.legend(loc='upper left')
    
    add_secondary_xaxis_by_N(ax, df)
    
    plt.tight_layout()
    plt.savefig(os.path.join(PLOTS_DIR, '02_metric_multigraph_by_N.png'))
    plt.close()


# ---------------------------------------------------------
# 3. EXACT ISOMORPHISM (Limited to N=18)
# ---------------------------------------------------------
def plot_isomorphism_exact():
    print("[INFO] Generating Exact Isomorphism plot (Naive vs Ullmann)...")
    df = load_data('isomorphism.csv')
    if df is None: return
    df = calculate_graph_size(df)
    
    # --- Plot 1: X-axis = |V|+|E| ---
    fig, ax = plt.subplots()
    
    target_algos = ['Naive', 'Ullmann'] 
    scaling_factors = []

    for algo in target_algos:
        subset = df[
            (df['Algorithm'].str.contains(algo, case=False)) & 
            (df['Size_N'] <= 18)
        ].sort_values(by='Graph_Size')
        
        subset = subset[subset['Avg_Time_ms'] > 0]
        
        if not subset.empty:
            ax.plot(subset['Graph_Size'], subset['Avg_Time_ms'], marker='o', label=f'Algorytm {algo}')
            
            last_point = subset.iloc[-1]
            try:
                n_val = int(last_point['Size_N'])
                t_val = last_point['Avg_Time_ms']
                fact_val = float(factorial(n_val))
                k = t_val / fact_val
                scaling_factors.append(k)
            except:
                pass

    if scaling_factors:
        try:
            k_avg = np.exp(np.mean(np.log(scaling_factors)))
            n_range = np.arange(5, 19)
            # Map theoretical N to Graph Size
            x_theo = n_range + np.floor(0.6 * n_range * (n_range - 1) / 2)
            y_theo = k_avg * factorial(n_range.astype(int)) + 200  # Offset for visibility
            
            ax.plot(x_theo, y_theo, color='black', linestyle=':', linewidth=2, 
                     label='Teoria O(N!) [Średni Fit]')
        except OverflowError:
            print("[WARNING] Factorial too large.")

    ax.set_title('Złożoność Algorytmów Dokładnych (Izomorfizm)')
    ax.set_xlabel('Rozmiar Grafu (|V| + |E|)')
    ax.set_ylabel('Czas wykonywania [ms] (Skala Log)')
    
    ax.set_yscale('log')
    ax.legend()
    
    # Filter df for top axis to match the plotted range (N<=18)
    df_filtered = df[df['Size_N'] <= 18]
    add_secondary_xaxis(ax, df_filtered)
    
    plt.tight_layout()
    plt.savefig(os.path.join(PLOTS_DIR, '03_isomorphism_exact.png'))
    plt.close()

    # --- Plot 2: X-axis = N ---
    fig, ax = plt.subplots()
    
    scaling_factors = []

    for algo in target_algos:
        subset = df[
            (df['Algorithm'].str.contains(algo, case=False)) & 
            (df['Size_N'] <= 18)
        ].sort_values(by='Size_N')
        
        subset = subset[subset['Avg_Time_ms'] > 0]
        
        if not subset.empty:
            ax.plot(subset['Size_N'], subset['Avg_Time_ms'], marker='o', label=f'Algorytm {algo}')
            
            last_point = subset.iloc[-1]
            try:
                n_val = int(last_point['Size_N'])
                t_val = last_point['Avg_Time_ms']
                fact_val = float(factorial(n_val))
                k = t_val / fact_val
                scaling_factors.append(k)
            except:
                pass

    if scaling_factors:
        try:
            k_avg = np.exp(np.mean(np.log(scaling_factors)))
            n_range = np.arange(5, 19)
            y_theo = k_avg * factorial(n_range.astype(int)) + 200  # Offset for visibility
            
            ax.plot(n_range, y_theo, color='black', linestyle=':', linewidth=2, 
                     label='Teoria O(N!) [Średni Fit]')
        except OverflowError:
            print("[WARNING] Factorial too large.")

    ax.set_title('Złożoność Algorytmów Dokładnych (Izomorfizm, wg N)')
    ax.set_xlabel('Liczba Wierzchołków (N)')
    ax.set_ylabel('Czas wykonywania [ms] (Skala Log)')
    
    ax.set_yscale('log')
    ax.legend()
    
    add_secondary_xaxis_by_N(ax, df_filtered)
    
    plt.tight_layout()
    plt.savefig(os.path.join(PLOTS_DIR, '03_isomorphism_exact_by_N.png'))
    plt.close()

# ---------------------------------------------------------
# 4. VF2 AVERAGE CASE (FV2)
# ---------------------------------------------------------
def plot_vf2_average_case():
    print("[INFO] Generating VF2 Average Case plot...")
    df = load_data('isomorphism_vf2.csv') 
    if df is None: return
    df = calculate_graph_size(df)

    # --- Plot 1: X-axis = |V|+|E| ---
    fig, ax = plt.subplots()
    
    subset = df[df['Algorithm'] == 'FV2'].sort_values(by='Graph_Size')
    
    if subset.empty:
        print("[WARNING] No data found for algorithm 'VF2'")
    else:
        ax.plot(subset['Graph_Size'], subset['Avg_Time_ms'], marker='D', linestyle='-', color='purple', label='Algorytm VF2')

        if len(subset) > 1:
            x = subset['Graph_Size']
            y = subset['Avg_Time_ms']
            slope, intercept = np.polyfit(x, y, 1)
            x_theo = np.linspace(x.min(), x.max(), 100)
            y_theo = slope * x_theo + intercept
            ax.plot(x_theo, y_theo, color='black', linestyle='--', label='Fit Liniowy (Average Case)')

    ax.set_title('Wydajność algorytmu VF2 (Przypadek Średni - Grafy Losowe)')
    ax.set_xlabel('Rozmiar Grafu (|V| + |E|)')
    ax.set_ylabel('Czas wykonywania [ms]')
    ax.legend()
    
    add_secondary_xaxis(ax, subset)
    
    plt.tight_layout()
    plt.savefig(os.path.join(PLOTS_DIR, '04_isomorphism_vf2_avg.png'))
    plt.close()

    # --- Plot 2: X-axis = N ---
    fig, ax = plt.subplots()
    
    subset = df[df['Algorithm'] == 'FV2'].sort_values(by='Size_N')
    
    if subset.empty:
        print("[WARNING] No data found for algorithm 'FV2'")
    else:
        ax.plot(subset['Size_N'], subset['Avg_Time_ms'], marker='D', linestyle='-', color='purple', label='Algorytm VF2')

        if len(subset) > 1:
            x = subset['Size_N']
            y = subset['Avg_Time_ms']
            coeffs = np.polyfit(x, y, 2)
            poly_eqn = np.poly1d(coeffs)
            x_theo = np.linspace(x.min(), x.max(), 100)
            y_theo = poly_eqn(x_theo)
            ax.plot(x_theo, y_theo, color='black', linestyle='--', label='Fit Kwadratowy O(N^2)')

    ax.set_title('Wydajność algorytmu VF2 (Przypadek Średni - Grafy Losowe, wg N)')
    ax.set_xlabel('Liczba Wierzchołków (N)')
    ax.set_ylabel('Czas wykonywania [ms]')
    ax.legend()
    
    add_secondary_xaxis_by_N(ax, subset)
    
    plt.tight_layout()
    plt.savefig(os.path.join(PLOTS_DIR, '04_isomorphism_vf2_avg_by_N.png'))
    plt.close()

# ---------------------------------------------------------
# 5. EXTENSION MULTIGRAPH APPROX TIME (Split TAP/LeRP)
# ---------------------------------------------------------
def plot_extension_multigraph_time():
    print("[INFO] Generating Extension Multigraph Time plots (TAP vs LeRP)...")
    df = load_data('extension_multigraph_time_summary.csv')
    if df is None: return
    
    df['Graph_Size'] = df['Size_N'] + df['Est_Edges']

    configs = [
        ('tap', 'tab:orange', 1, 'Teoria O(N^2) ~ Linear vs Size', 2, 'Teoria O(N^2)'),
        ('lerp', 'tab:blue', 2, 'Teoria O(N^4) ~ Quadratic vs Size', 4, 'Teoria O(N^4)')
    ]

    for algo, color, deg_size, theory_label_size, deg_n, theory_label_n in configs:
        algo_data = df[df['Algorithm'] == algo]
        weights = algo_data['Max_Weight'].unique()
        # --- Plot 1: X-axis = |V|+|E| ---
        fig, ax = plt.subplots()
        
        for w in sorted(weights):
            subset = algo_data[algo_data['Max_Weight'] == w].sort_values(by='Graph_Size')
            linestyle = '-' if w == 1 else '--'
            marker = 'o' if algo == 'tap' else 's'
            label = f"{algo.upper()} (W={w})"
            
            ax.plot(subset['Graph_Size'], subset['Avg_Time_ms'], 
                     marker=marker, linestyle=linestyle, color=color, 
                     label=label, alpha=0.8)

        x_all = algo_data['Graph_Size']
        y_all = algo_data['Avg_Time_ms']
        
        if len(x_all) > 1:
            coeffs = np.polyfit(x_all, y_all, deg_size)
            poly_eqn = np.poly1d(coeffs)
            x_theo = np.linspace(x_all.min(), x_all.max(), 100)
            y_theo = poly_eqn(x_theo)
            
            ax.plot(x_theo, y_theo, color='black', linestyle=':', linewidth=2, 
                     label=theory_label_size)

        ax.set_title(f'Wydajność aproksymacji {algo.upper()} (Extension)')
        ax.set_xlabel('Rozmiar Grafu (|V| + |E|)')
        ax.set_ylabel('Czas wykonywania [ms]')
        ax.legend()
        
        add_secondary_xaxis(ax, algo_data)
        
        filename = f'05_{algo}_extension_multigraph_time.png'
        plt.tight_layout()
        plt.savefig(os.path.join(PLOTS_DIR, filename))
        print(f"   Saved to: {filename}")
        plt.close()

        # --- Plot 2: X-axis = N ---
        fig, ax = plt.subplots()

        for w in sorted(weights):
            subset = algo_data[algo_data['Max_Weight'] == w].sort_values(by='Size_N')
            linestyle = '-' if w == 1 else '--'
            marker = 'o' if algo == 'tap' else 's'
            label = f"{algo.upper()} (W={w})"
            
            ax.plot(subset['Size_N'], subset['Avg_Time_ms'], 
                     marker=marker, linestyle=linestyle, color=color, 
                     label=label, alpha=0.8)

        x_all = algo_data['Size_N']
        y_all = algo_data['Avg_Time_ms']
        
        if len(x_all) > 1:
            coeffs = np.polyfit(x_all, y_all, deg_n)
            poly_eqn = np.poly1d(coeffs)
            x_theo = np.linspace(x_all.min(), x_all.max(), 100)
            y_theo = poly_eqn(x_theo)
            
            ax.plot(x_theo, y_theo, color='black', linestyle=':', linewidth=2, 
                     label=theory_label_n)

        ax.set_title(f'Wydajność aproksymacji {algo.upper()} (Extension, wg N)')
        ax.set_xlabel('Liczba Wierzchołków (N)')
        ax.set_ylabel('Czas wykonywania [ms]')
        ax.legend()
        
        add_secondary_xaxis_by_N(ax, algo_data)
        
        filename_by_n = f'05_{algo}_extension_multigraph_time_by_N.png'
        plt.tight_layout()
        plt.savefig(os.path.join(PLOTS_DIR, filename_by_n))
        print(f"   Saved to: {filename_by_n}")
        plt.close()

# --- MAIN EXECUTION ---
if __name__ == "__main__":
    print(f"Starting plot generation reading from '{RESULTS_DIR}'...")
    
    try:
        plot_metric_linearity()
        plot_metric_multigraph()
        plot_isomorphism_exact()
        plot_vf2_average_case()
        plot_extension_multigraph_time()
    except Exception as e:
        print(f"[ERROR] An unexpected error occurred: {e}")
        import traceback
        traceback.print_exc()
    
    print(f"\nAll done! Check the '{PLOTS_DIR}' folder.")