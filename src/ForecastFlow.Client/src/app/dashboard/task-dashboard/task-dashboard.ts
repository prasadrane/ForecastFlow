import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../auth/auth.service';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-task-dashboard',
  standalone: false,
  templateUrl: './task-dashboard.html',
  styleUrl: './task-dashboard.css'
})
export class TaskDashboard implements OnInit {
  tasks: any[] = [];
  forecasts: any[] = [];
  userInfo: any;
  isLoading = false;

  constructor(
    private authService: AuthService,
    private apiService: ApiService
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading = true;
    this.userInfo = this.authService.getUserInfo();

    // Load tasks and forecasts concurrently
    Promise.all([
      this.loadTasks(),
      this.loadForecasts()
    ]).finally(() => {
      this.isLoading = false;
    });
  }

  private loadTasks(): Promise<void> {
    return new Promise((resolve) => {
      this.apiService.getTasks({ limit: 5 }).subscribe({
        next: (response) => {
          this.tasks = response.data || [];
          resolve();
        },
        error: (error) => {
          console.error('Failed to load tasks:', error);
          this.tasks = [];
          resolve();
        }
      });
    });
  }

  private loadForecasts(): Promise<void> {
    return new Promise((resolve) => {
      this.apiService.getForecasts({ limit: 5 }).subscribe({
        next: (response) => {
          this.forecasts = response.data || [];
          resolve();
        },
        error: (error) => {
          console.error('Failed to load forecasts:', error);
          this.forecasts = [];
          resolve();
        }
      });
    });
  }

  onLogout(): void {
    this.authService.logout().subscribe({
      next: () => {
        // Logout successful, navigation is handled by AuthService
      },
      error: (error) => {
        console.error('Logout error:', error);
        // Even if logout fails on server, user is logged out locally
      }
    });
  }
}
