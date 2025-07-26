import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, switchMap, filter, take } from 'rxjs/operators';
import { AuthService } from '../auth/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

  constructor(private authService: AuthService) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Don't add auth header to login, register, or refresh token requests
    if (this.isAuthRequest(request.url)) {
      return next.handle(request);
    }

    // Add auth header if user is authenticated
    if (this.authService.isAuthenticated()) {
      request = this.addTokenHeader(request, this.authService.getToken());
    }

    return next.handle(request).pipe(
      catchError(error => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          return this.handle401Error(request, next);
        }
        return throwError(() => error);
      })
    );
  }

  private addTokenHeader(request: HttpRequest<any>, token: string | null): HttpRequest<any> {
    if (token) {
      return request.clone({
        headers: request.headers.set('Authorization', `Bearer ${token}`)
      });
    }
    return request;
  }

  private handle401Error(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      const refreshToken = this.authService.getRefreshToken();
      if (refreshToken) {
        return this.authService.refreshToken().pipe(
          switchMap((success: boolean) => {
            this.isRefreshing = false;
            if (success) {
              this.refreshTokenSubject.next(this.authService.getToken());
              return next.handle(this.addTokenHeader(request, this.authService.getToken()));
            } else {
              // Refresh failed, redirect to login
              return throwError(() => new Error('Token refresh failed'));
            }
          }),
          catchError((error) => {
            this.isRefreshing = false;
            // Refresh failed, redirect to login is handled by AuthService
            return throwError(() => error);
          })
        );
      } else {
        // No refresh token available, redirect to login
        this.isRefreshing = false;
        return throwError(() => new Error('No refresh token available'));
      }
    }

    // If we're already refreshing, wait for the new token
    return this.refreshTokenSubject.pipe(
      filter(token => token !== null),
      take(1),
      switchMap((token) => next.handle(this.addTokenHeader(request, token)))
    );
  }

  private isAuthRequest(url: string): boolean {
    return url.includes('/auth/login') || 
           url.includes('/auth/register') || 
           url.includes('/auth/refresh');
  }
}
