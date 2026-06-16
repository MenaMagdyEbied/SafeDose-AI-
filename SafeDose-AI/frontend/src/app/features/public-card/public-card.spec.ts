import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PublicCard } from './public-card';

describe('PublicCard', () => {
  let component: PublicCard;
  let fixture: ComponentFixture<PublicCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PublicCard],
    }).compileComponents();

    fixture = TestBed.createComponent(PublicCard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
