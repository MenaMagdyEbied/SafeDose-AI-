import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DigitalCard } from './digital-card';

describe('DigitalCard', () => {
  let component: DigitalCard;
  let fixture: ComponentFixture<DigitalCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DigitalCard],
    }).compileComponents();

    fixture = TestBed.createComponent(DigitalCard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
