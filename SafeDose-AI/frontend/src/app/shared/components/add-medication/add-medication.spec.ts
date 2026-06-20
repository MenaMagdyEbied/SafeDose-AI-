import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddMedication } from './add-medication';

describe('AddMedication', () => {
  let component: AddMedication;
  let fixture: ComponentFixture<AddMedication>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddMedication],
    }).compileComponents();

    fixture = TestBed.createComponent(AddMedication);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
