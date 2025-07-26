import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskDashboard } from './task-dashboard';

describe('TaskDashboard', () => {
  let component: TaskDashboard;
  let fixture: ComponentFixture<TaskDashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TaskDashboard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TaskDashboard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
