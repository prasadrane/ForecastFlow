import { Component } from '@angular/core';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  username = '';
  password = '';
  errorMessage = '';

  constructor(private authService: AuthService) {}

  onLogin(): void {
    if (this.authService.login(this.username, this.password)) {
      this.errorMessage = '';
    } else {
      this.errorMessage = 'Invalid credentials. Please try again.';
    }
  }
}
