import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { 
  LoginRequest, 
  LoginResponse, 
  RefreshTokenRequest, 
  RefreshTokenResponse,
  Task,
  Forecast,
  User,
  ApiResponse,
  PaginatedResponse
} from '../models/interfaces';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly baseUrl: string;

  constructor(private http: HttpClient) {
    // Use environment configuration or fallback to default
    this.baseUrl = environment?.apiUrl || 'https://localhost:7058/api';
  }

  /**
   * Get the authorization headers with JWT token if available
   */
  private getAuthHeaders(): HttpHeaders {
    let headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    const token = localStorage.getItem('jwt_token');
    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }

    return headers;
  }

  /**
   * Generic GET request
   */
  get<T>(endpoint: string, params?: any): Observable<T> {
    let httpParams = new HttpParams();
    
    if (params) {
      Object.keys(params).forEach(key => {
        if (params[key] !== null && params[key] !== undefined) {
          httpParams = httpParams.set(key, params[key]);
        }
      });
    }

    return this.http.get<T>(`${this.baseUrl}/${endpoint}`, {
      headers: this.getAuthHeaders(),
      params: httpParams
    });
  }

  /**
   * Generic POST request
   */
  post<T>(endpoint: string, data: any): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}/${endpoint}`, data, {
      headers: this.getAuthHeaders()
    });
  }

  /**
   * Generic PUT request
   */
  put<T>(endpoint: string, data: any): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}/${endpoint}`, data, {
      headers: this.getAuthHeaders()
    });
  }

  /**
   * Generic PATCH request
   */
  patch<T>(endpoint: string, data: any): Observable<T> {
    return this.http.patch<T>(`${this.baseUrl}/${endpoint}`, data, {
      headers: this.getAuthHeaders()
    });
  }

  /**
   * Generic DELETE request
   */
  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}/${endpoint}`, {
      headers: this.getAuthHeaders()
    });
  }

  /**
   * File upload request
   */
  upload<T>(endpoint: string, file: File, additionalData?: any): Observable<T> {
    const formData = new FormData();
    formData.append('file', file);
    
    if (additionalData) {
      Object.keys(additionalData).forEach(key => {
        formData.append(key, additionalData[key]);
      });
    }

    // Don't set Content-Type header for FormData, let browser set it with boundary
    let headers = new HttpHeaders();
    const token = localStorage.getItem('jwt_token');
    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }

    return this.http.post<T>(`${this.baseUrl}/${endpoint}`, formData, {
      headers: headers
    });
  }

  /**
   * Authentication specific methods
   */
  
  /**
   * Login request (no auth header needed)
   */
  login(credentials: LoginRequest): Observable<LoginResponse> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/login`, credentials, {
      headers: headers
    });
  }

  /**
   * Register request (no auth header needed)
   */
  register(userData: any): Observable<ApiResponse<User>> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    return this.http.post<ApiResponse<User>>(`${this.baseUrl}/auth/register`, userData, {
      headers: headers
    });
  }

  /**
   * Refresh token request
   */
  refreshToken(refreshToken: string): Observable<RefreshTokenResponse> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    return this.http.post<RefreshTokenResponse>(`${this.baseUrl}/auth/refresh`, { refreshToken }, {
      headers: headers
    });
  }

  /**
   * Logout request
   */
  logout(): Observable<any> {
    return this.http.post(`${this.baseUrl}/auth/logout`, {}, {
      headers: this.getAuthHeaders()
    });
  }

  /**
   * Forecast specific methods
   */
  
  /**
   * Get all forecasts
   */
  getForecasts(params?: any): Observable<PaginatedResponse<Forecast>> {
    return this.get<PaginatedResponse<Forecast>>('forecasts', params);
  }

  /**
   * Get forecast by ID
   */
  getForecast(id: number): Observable<ApiResponse<Forecast>> {
    return this.get<ApiResponse<Forecast>>(`forecasts/${id}`);
  }

  /**
   * Create new forecast
   */
  createForecast(forecastData: Partial<Forecast>): Observable<ApiResponse<Forecast>> {
    return this.post<ApiResponse<Forecast>>('forecasts', forecastData);
  }

  /**
   * Update forecast
   */
  updateForecast(id: number, forecastData: Partial<Forecast>): Observable<ApiResponse<Forecast>> {
    return this.put<ApiResponse<Forecast>>(`forecasts/${id}`, forecastData);
  }

  /**
   * Delete forecast
   */
  deleteForecast(id: number): Observable<ApiResponse<void>> {
    return this.delete<ApiResponse<void>>(`forecasts/${id}`);
  }

  /**
   * Task specific methods
   */
  
  /**
   * Get all tasks
   */
  getTasks(params?: any): Observable<PaginatedResponse<Task>> {
    return this.get<PaginatedResponse<Task>>('tasks', params);
  }

  /**
   * Get task by ID
   */
  getTask(id: number): Observable<ApiResponse<Task>> {
    return this.get<ApiResponse<Task>>(`tasks/${id}`);
  }

  /**
   * Create new task
   */
  createTask(taskData: Partial<Task>): Observable<ApiResponse<Task>> {
    return this.post<ApiResponse<Task>>('tasks', taskData);
  }

  /**
   * Update task
   */
  updateTask(id: number, taskData: Partial<Task>): Observable<ApiResponse<Task>> {
    return this.put<ApiResponse<Task>>(`tasks/${id}`, taskData);
  }

  /**
   * Delete task
   */
  deleteTask(id: number): Observable<ApiResponse<void>> {
    return this.delete<ApiResponse<void>>(`tasks/${id}`);
  }

  /**
   * Get user profile
   */
  getUserProfile(): Observable<ApiResponse<User>> {
    return this.get<ApiResponse<User>>('user/profile');
  }

  /**
   * Update user profile
   */
  updateUserProfile(profileData: Partial<User>): Observable<ApiResponse<User>> {
    return this.put<ApiResponse<User>>('user/profile', profileData);
  }
}
