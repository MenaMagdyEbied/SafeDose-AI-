import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminManager } from './admin-manager';

describe('AdminManager', () => {
  let component: AdminManager;
  let fixture: ComponentFixture<AdminManager>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminManager],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminManager);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
