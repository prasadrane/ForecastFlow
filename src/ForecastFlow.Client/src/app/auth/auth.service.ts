import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { ApiService } from '../services/api.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private authenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.authenticatedSubject.asObservable();

  constructor(
    private router: Router,
    private apiService: ApiService
  ) {
    // Check if user is already authenticated (check for JWT token)
    this.checkAuthenticationStatus();
  }

  private checkAuthenticationStatus(): void {
    const token = localStorage.getItem('jwt_token');
    const isAuthenticated = localStorage.getItem('isAuthenticated') === 'true';
    
    if (token && isAuthenticated) {
      this.authenticatedSubject.next(true);
    } else {
      this.authenticatedSubject.next(false);
    }
  }

  login(username: string, password: string): Observable<boolean> {
    return this.apiService.login({ username, password }).pipe(
      tap(response => {
        // Successful login - store token and user info
        if (response.token) {
          localStorage.setItem('jwt_token', response.token);
          localStorage.setItem('isAuthenticated', 'true');
          
          if (response.refreshToken) {
            localStorage.setItem('refresh_token', response.refreshToken);
          }
          
          if (response.user) {
            localStorage.setItem('user_info', JSON.stringify(response.user));
          }

          this.authenticatedSubject.next(true);
          this.router.navigate(['/dashboard']);
        }
      }),
      map(response => !!response.token),
      catchError(error => {
        console.error('Login failed:', error);
        this.authenticatedSubject.next(false);
        return of(false);
      })
    );
  }

  logout(): Observable<void> {
    return this.apiService.logout().pipe(
      tap(() => {
        this.clearAuthData();
      }),
      catchError(error => {
        console.error('Logout error:', error);
        // Even if the server request fails, clear local data
        this.clearAuthData();
        return of(void 0);
      })
    );
  }

  private clearAuthData(): void {
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('isAuthenticated');
    localStorage.removeItem('user_info');
    this.authenticatedSubject.next(false);
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    const token = localStorage.getItem('jwt_token');
    const isAuthenticated = localStorage.getItem('isAuthenticated') === 'true';
    return !!(token && isAuthenticated);
  }

  getToken(): string | null {
    return localStorage.getItem('jwt_token');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refresh_token');
  }

  getUserInfo(): any {
    const userInfo = localStorage.getItem('user_info');
    return userInfo ? JSON.parse(userInfo) : null;
  }

  refreshToken(): Observable<boolean> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      this.clearAuthData();
      return of(false);
    }

    return this.apiService.refreshToken(refreshToken).pipe(
      tap(response => {
        if (response.token) {
          localStorage.setItem('jwt_token', response.token);
          if (response.refreshToken) {
            localStorage.setItem('refresh_token', response.refreshToken);
          }
          this.authenticatedSubject.next(true);
        } else {
          this.clearAuthData();
        }
      }),
      map(response => !!response.token),
      catchError(error => {
        console.error('Token refresh failed:', error);
        this.clearAuthData();
        return of(false);
      })
    );
  }
}
