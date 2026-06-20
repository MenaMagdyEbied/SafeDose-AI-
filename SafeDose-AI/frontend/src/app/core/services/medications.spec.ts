import { TestBed } from '@angular/core/testing';

import { Medications } from './medications';

describe('Medications', () => {
  let service: Medications;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Medications);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
