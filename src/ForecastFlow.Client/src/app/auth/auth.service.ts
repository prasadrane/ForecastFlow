import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private authenticated = false;

  constructor(private router: Router) {
    // Check if user is already authenticated (e.g., from localStorage)
    this.authenticated = localStorage.getItem('isAuthenticated') === 'true';
  }

  login(username: string, password: string): boolean {
    // This is a simple mock authentication
    // In a real application, you would validate credentials against a backend
    if (username && password) {
      this.authenticated = true;
      localStorage.setItem('isAuthenticated', 'true');
      this.router.navigate(['/dashboard']);
      return true;
    }
    return false;
  }

  logout(): void {
    this.authenticated = false;
    localStorage.removeItem('isAuthenticated');
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    return this.authenticated;
  }
}
