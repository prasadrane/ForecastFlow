import { Component } from '@angular/core';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-task-dashboard',
  standalone: false,
  templateUrl: './task-dashboard.html',
  styleUrl: './task-dashboard.css'
})
export class TaskDashboard {

  constructor(private authService: AuthService) {}

  onLogout(): void {
    this.authService.logout();
  }
}
