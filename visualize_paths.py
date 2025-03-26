import json
import matplotlib.pyplot as plt
import numpy as np
from matplotlib.patches import Polygon, Patch
from matplotlib.colors import LinearSegmentedColormap
from matplotlib.lines import Line2D

def objective_function(X1, X2):
    X3 = 1 - X1 - X2
    return -(X1 * X2 * X3) / 8

def create_contour_plot(ax):
    x = np.linspace(0, 1, 100)
    y = np.linspace(0, 1, 100)
    X, Y = np.meshgrid(x, y)
    Z = np.zeros_like(X)
    
    for i in range(X.shape[0]):
        for j in range(X.shape[1]):
            if X[i,j] + Y[i,j] <= 1:  
                Z[i,j] = objective_function(X[i,j], Y[i,j])
    
    contour = ax.contourf(X, Y, Z, levels=20, cmap='viridis', alpha=0.5)
    plt.colorbar(contour, ax=ax, label='Objective Value')
    
    boundary_x = np.linspace(0, 1, 100)
    boundary_y = 1 - boundary_x
    ax.plot(boundary_x, boundary_y, 'r--', linewidth=2)
    
    ax.scatter(1/3, 1/3, color='gold', marker='*', s=200, zorder=5)
    ax.set_xlabel('X1')
    ax.set_ylabel('X2')
    ax.grid(alpha=0.3)
    return ax

def plot_optimization_paths(ax, data, start_point):
    method_styles = {
        'GradientLipschitz': {'color': 'blue', 'marker': 'o', 'linestyle': '-'},
        'SteepestDescent': {'color': 'green', 'marker': 's', 'linestyle': '--'},
        'DeformedSimplex': {'color': 'red', 'marker': 'D', 'linestyle': '-.'}
    }
    
    for entry in data:
        if not isinstance(entry, dict) or entry.get('StartPoint') != start_point:
            continue
            
        method = entry.get('Method', 'Unknown')
        style = method_styles.get(method, {})
        points = []
        
        for p in entry.get('Path', []):
            if isinstance(p, dict):
                x1 = p.get('X1') or p.get('Item1') or 0
                x2 = p.get('X2') or p.get('Item2') or 0
                points.append((x1, x2))
            elif isinstance(p, (list, tuple)) and len(p) >= 2:
                points.append((p[0], p[1]))
        
        if not points:
            continue
            
        x1, x2 = zip(*points)
        
        ax.plot(x1, x2, color=style['color'], linestyle=style['linestyle'],
               linewidth=2, alpha=0.8)
        
        ax.scatter(x1, x2, color=style['color'], marker=style['marker'], s=40)
        
        if method == "DeformedSimplex" and len(points) >= 3:
            for i in range(2, len(points)):
                triangle = Polygon([points[i-2], points[i-1], points[i]], 
                                  closed=True, alpha=0.1, color=style['color'])
                ax.add_patch(triangle)
    
    for entry in data:
        if not isinstance(entry, dict) or entry.get('StartPoint') != start_point:
            continue
            
        method = entry.get('Method', 'Unknown')
        style = method_styles.get(method, {})
        points = []
        
        for p in entry.get('Path', []):
            if isinstance(p, dict):
                x1 = p.get('X1') or p.get('Item1') or 0
                x2 = p.get('X2') or p.get('Item2') or 0
                points.append((x1, x2))
            elif isinstance(p, (list, tuple)) and len(p) >= 2:
                points.append((p[0], p[1]))
        
        if not points:
            continue
            
        x1, x2 = zip(*points)
        
        ax.scatter(x1[0], x2[0], color=style['color'], marker=style['marker'],
                  s=150, edgecolors='k', zorder=4)
        ax.scatter(x1[-1], x2[-1], color=style['color'], marker=style['marker'],
                  s=150, edgecolors='k', zorder=4)
    
    return ax

def create_legend_elements():
    elements = [
        Line2D([0], [0], color='r', linestyle='--', label='Feasible Boundary'),
        Line2D([0], [0], marker='*', color='gold', markersize=10, 
              label='Optimum (1/3, 1/3)', linestyle='None'),
        
        Line2D([0], [0], color='blue', linestyle='-', label='Gradient Path'),
        Line2D([0], [0], color='green', linestyle='--', label='Steepest Path'),
        Line2D([0], [0], color='red', linestyle='-.', label='Simplex Path'),
        
        Line2D([0], [0], marker='o', color='blue', markersize=8, 
              label='Gradient Points', linestyle='None'),
        Line2D([0], [0], marker='s', color='green', markersize=8, 
              label='Steepest Points', linestyle='None'),
        Line2D([0], [0], marker='D', color='red', markersize=8, 
              label='Simplex Points', linestyle='None'),
        
        Line2D([0], [0], marker='o', color='k', markersize=10, markerfacecolor='w',
              label='Start Points', linestyle='None'),
        Line2D([0], [0], marker='s', color='k', markersize=10, markerfacecolor='w',
              label='End Points', linestyle='None'),
        
        Patch(facecolor='red', alpha=0.1, label='Simplex Triangles')
    ]
    return elements

def create_start_point_visualization(data, start_point):
    fig, ax = plt.subplots(figsize=(12, 8))
    
    ax = create_contour_plot(ax)
    
    ax = plot_optimization_paths(ax, data, start_point)
    
    legend_elements = create_legend_elements()
    legend = ax.legend(
        handles=create_legend_elements(),
        bbox_to_anchor=(1.25, 1),  
        loc='upper left',
        ncol=2,  
        fontsize=9,
        borderaxespad=0.5,
        framealpha=0.9  
    )

    
    ax.set_title(f'Optimization Paths from {start_point}', pad=20)
    plt.tight_layout()
    plt.savefig(f'{start_point}_optimizations.png', dpi=300, bbox_inches='tight')
    plt.close()

def main(json_file):
    with open(json_file) as f:
        data = json.load(f)
    
    start_points = ['X0', 'X1', 'Xm']
    for sp in start_points:
        create_start_point_visualization(data, sp)
    
    print("Visualization complete. Created files:")
    for sp in start_points:
        print(f"- {sp}_optimizations.png")

if __name__ == "__main__":
    import sys
    if len(sys.argv) != 2:
        print("Usage: python visualize_paths.py <json_file>")
        sys.exit(1)
    
    main(sys.argv[1])
