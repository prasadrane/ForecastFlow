import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class TaskService {
    private apiUrl = 'http://localhost:5089/api/tasks';

    constructor(private http: HttpClient) { }

    getAll(): Observable<any[]> {
        return this.http.get<any[]>(this.apiUrl);
    }

    getById(id: number): Observable<any> {
        return this.http.get<any>(`${this.apiUrl}/${id}`);
    }

    create(task: any): Observable<any> {
        return this.http.post(this.apiUrl, task);
    }

    update(id: number, task: any): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}`, task);
    }

    delete(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/${id}`);
    }
}
