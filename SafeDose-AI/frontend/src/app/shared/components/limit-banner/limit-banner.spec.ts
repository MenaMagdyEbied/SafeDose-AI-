import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LimitBanner } from './limit-banner';

describe('LimitBanner', () => {
  let component: LimitBanner;
  let fixture: ComponentFixture<LimitBanner>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LimitBanner],
    }).compileComponents();

    fixture = TestBed.createComponent(LimitBanner);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
