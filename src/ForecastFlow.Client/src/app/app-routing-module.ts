import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Login } from './auth/login/login';
import { TaskDashboard } from './dashboard/task-dashboard/task-dashboard';
import { AuthGuard } from './auth/auth.guard';

const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'login', component: Login },
  { path: 'dashboard', component: TaskDashboard, canActivate: [AuthGuard] },
  { path: '**', redirectTo: '/dashboard' } // Wildcard route for 404 page
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
