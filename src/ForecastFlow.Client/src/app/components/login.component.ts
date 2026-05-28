import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  template: `
    <div class="login-container">
      <mat-card>
        <mat-card-header>
          <mat-card-title>{{isRegister ? 'Register' : 'Login'}}</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
            <mat-form-field appearance="fill">
              <mat-label>Username</mat-label>
              <input matInput formControlName="username">
            </mat-form-field>
            <mat-form-field appearance="fill" *ngIf="isRegister">
              <mat-label>Email</mat-label>
              <input matInput type="email" formControlName="email">
            </mat-form-field>
            <mat-form-field appearance="fill">
              <mat-label>Password</mat-label>
              <input matInput type="password" formControlName="password">
            </mat-form-field>
            <button mat-raised-button color="primary" [disabled]="loading">{{isRegister ? 'Register' : 'Login'}}</button>
            <a mat-button [routerLink]="isRegister ? '/login' : '/register'">
              {{isRegister ? 'Already have an account? Login' : 'Need an account? Register'}}
            </a>
            <div *ngIf="error" class="error">{{error}}</div>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
    <style>
      .login-container { display: flex; justify-content: center; margin-top: 50px; }
      mat-card { width: 400px; }
      form { display: flex; flex-direction: column; gap: 10px; }
      .error { color: red; margin-top: 10px; }
    </style>
  `,
  standalone: false
})
export class LoginComponent {
  loginForm: FormGroup;
  loading = false;
  error = '';
  isRegister = false;

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private authService: AuthService
  ) {
    this.isRegister = this.router.url === '/register';
    this.loginForm = this.formBuilder.group({
      username: ['', Validators.required],
      email: [''],
      password: ['', Validators.required]
    });
    if (this.isRegister) {
      this.loginForm.get('email')?.setValidators([Validators.required, Validators.email]);
    }
  }

  onSubmit() {
    if (this.loginForm.invalid) return;
    this.loading = true;
    this.error = '';

    if (this.isRegister) {
      this.authService.register(this.loginForm.value).subscribe({
        next: () => {
          this.authService.login(this.loginForm.value.username, this.loginForm.value.password)
            .subscribe({
              next: () => this.router.navigate(['/']),
              error: () => {
                this.error = 'Login failed after registration';
                this.loading = false;
              }
            });
        },
        error: err => {
          this.error = 'Registration failed. Username might be taken.';
          this.loading = false;
        }
      });
    } else {
      this.authService.login(this.loginForm.value.username, this.loginForm.value.password)
        .subscribe({
          next: () => this.router.navigate(['/']),
          error: err => {
            this.error = 'Invalid credentials';
            this.loading = false;
          }
        });
    }
  }
}
