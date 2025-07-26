import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Login } from './login/login';

@NgModule({
  declarations: [
    Login
  ],
  imports: [
    CommonModule,
    FormsModule
  ]
})
export class AuthModule { }
