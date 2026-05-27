import { Component, OnInit, OnDestroy } from '@angular/core';
import { TaskService } from '../services/task.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  template: `
    <div class="dashboard">
      <h1>My Tasks</h1>

      <mat-card class="task-form">
        <mat-card-header><mat-card-title>Add New Task</mat-card-title></mat-card-header>
        <mat-card-content>
          <form [formGroup]="taskForm" (ngSubmit)="addTask()">
            <mat-form-field appearance="fill"><mat-label>Title</mat-label><input matInput formControlName="title"></mat-form-field>
            <mat-form-field appearance="fill"><mat-label>Location</mat-label><input matInput formControlName="locationName"></mat-form-field>
            <div class="coords">
              <mat-form-field appearance="fill"><mat-label>Lat</mat-label><input matInput type="number" formControlName="latitude"></mat-form-field>
              <mat-form-field appearance="fill"><mat-label>Lon</mat-label><input matInput type="number" formControlName="longitude"></mat-form-field>
            </div>
            <mat-form-field appearance="fill"><mat-label>Date</mat-label><input matInput type="datetime-local" formControlName="taskDateTime"></mat-form-field>
            <button mat-raised-button color="accent">Create Task</button>
          </form>
        </mat-card-content>
      </mat-card>

      <div class="task-list">
        <mat-card *ngFor="let task of tasks" class="task-item">
          <mat-card-header>
            <mat-card-title>{{task.title}}</mat-card-title>
            <mat-card-subtitle>{{task.locationName}} - {{task.taskDateTime | date:'medium'}}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <p>{{task.description}}</p>
            <div *ngIf="task.suggestionsJson" class="suggestions">
              <h3>Weather Suggestions:</h3>
              <ul>
                <li *ngFor="let suggestion of parseSuggestions(task.suggestionsJson)">{{suggestion}}</li>
              </ul>
            </div>
            <div *ngIf="!task.suggestionsJson" class="pending">
              <mat-spinner diameter="20"></mat-spinner> Getting weather suggestions...
            </div>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
    <style>
      .dashboard { display: flex; flex-direction: column; gap: 20px; }
      .task-form { max-width: 600px; }
      form { display: flex; flex-direction: column; gap: 10px; }
      .coords { display: flex; gap: 10px; }
      .task-list { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 20px; }
      .suggestions { background: #f0f4f8; padding: 10px; border-radius: 4px; margin-top: 10px; }
      .pending { display: flex; align-items: center; gap: 10px; font-style: italic; color: #666; }
    </style>
  `,
  standalone: false
})
export class DashboardComponent implements OnInit, OnDestroy {
  tasks: any[] = [];
  taskForm: FormGroup;
  private pollInterval: any;

  constructor(private taskService: TaskService, private fb: FormBuilder) {
    this.taskForm = this.fb.group({
      title: ['', Validators.required],
      locationName: ['London', Validators.required],
      latitude: [51.5074, Validators.required],
      longitude: [-0.1278, Validators.required],
      taskDateTime: [new Date().toISOString().slice(0, 16), Validators.required],
      description: ['']
    });
  }

  ngOnInit() {
    this.loadTasks();
    // Refresh periodically to get new suggestions
    this.pollInterval = setInterval(() => this.loadTasks(), 5000);
  }

  ngOnDestroy() {
    if (this.pollInterval) {
      clearInterval(this.pollInterval);
    }
  }

  loadTasks() {
    this.taskService.getAll().subscribe(data => this.tasks = data);
  }

  addTask() {
    if (this.taskForm.invalid) return;
    this.taskService.create(this.taskForm.value).subscribe(() => {
      this.taskForm.reset({
        locationName: 'London',
        latitude: 51.5074,
        longitude: -0.1278,
        taskDateTime: new Date().toISOString().slice(0, 16)
      });
      this.loadTasks();
    });
  }

  parseSuggestions(json: string) {
    try { return JSON.parse(json); } catch { return []; }
  }
}
