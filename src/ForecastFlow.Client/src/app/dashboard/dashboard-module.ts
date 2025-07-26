import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskDashboard } from './task-dashboard/task-dashboard';



@NgModule({
  declarations: [
    TaskDashboard
  ],
  imports: [
    CommonModule
  ]
})
export class DashboardModule { }
