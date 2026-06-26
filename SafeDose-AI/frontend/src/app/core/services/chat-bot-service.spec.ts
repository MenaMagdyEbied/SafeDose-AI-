import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';

import { PatientService } from './patient';
import { ChatBotService } from './chat-bot-service';

describe('ChatBotService', () => {
  let service: ChatBotService;
  let patientService: PatientService;

  beforeEach(() => {
    const patientServiceStub = {
      runningPatient: signal<any>(null),
      getPrimaryPatientId: jasmine.createSpy().and.returnValue(Promise.resolve(null)),
      setRunningPatient: jasmine.createSpy().and.returnValue(Promise.resolve()),
      get currentPatientId() {
        const patient = this.runningPatient();
        return patient?.patientId ?? patient?.id ?? null;
      },
    };

    TestBed.configureTestingModule({
      providers: [{ provide: PatientService, useValue: patientServiceStub }],
    });

    service = TestBed.inject(ChatBotService);
    patientService = TestBed.inject(PatientService);
  });

  it('should follow the active patient from the shared patient service', () => {
    (patientService as any).runningPatient.set({ patientId: 77 });

    expect((service as any).selectedPatientId).toBe(77);
  });
});
