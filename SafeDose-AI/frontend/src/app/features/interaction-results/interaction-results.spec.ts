import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InteractionResults } from './interaction-results';

describe('InteractionResults', () => {
  let component: InteractionResults;
  let fixture: ComponentFixture<InteractionResults>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InteractionResults],
    }).compileComponents();

    fixture = TestBed.createComponent(InteractionResults);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
