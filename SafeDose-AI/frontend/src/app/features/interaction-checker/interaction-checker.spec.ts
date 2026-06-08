import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InteractionChecker } from './interaction-checker';

describe('InteractionChecker', () => {
  let component: InteractionChecker;
  let fixture: ComponentFixture<InteractionChecker>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InteractionChecker],
    }).compileComponents();

    fixture = TestBed.createComponent(InteractionChecker);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
