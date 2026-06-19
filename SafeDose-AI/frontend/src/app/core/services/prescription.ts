import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class Prescription {
  prescriptions = signal<any[]>([
    {
      id: 1,
      name: 'وصفة د. محمد السيد',
      date: '١٥/٠٥/٢٠٢٥',
      source: 'scan',
      meds: [
        {
          name: 'بانادول اكسترا',
          dose: '٥٠٠ ملغ - قرص واحد',
          frequency: '٣ مرات يومياً',
          duration: 'لمدة ٥ أيام',
          warning: 'تعارض محتمل مع دواء وارفارين. راجع الطبيب.',
          chemicalName: 'Paracetamol + Caffeine',
          registryCode: 'EDA-REG-109283-PAN-01',
        },
        {
          name: 'ميتفورمين',
          dose: '٥٠٠ ملغ',
          frequency: 'مرتين يومياً',
          duration: 'مستمر',
          warning: '',
          chemicalName: 'Metformin Hydrochloride',
          registryCode: 'EDA-REG-204811-MET-02',
        },
        {
          name: 'أملوديبين',
          dose: '٥ ملغ',
          frequency: 'مرة يومياً',
          duration: 'مستمر',
          warning: '',
          chemicalName: 'Amlodipine Besylate',
          registryCode: 'EDA-REG-334521-AML-01',
        },
      ],
    },
    {
      id: 2,
      name: 'وصفة د. سارة أحمد',
      date: '٢٠/٠٤/٢٠٢٥',
      source: 'scan',
      meds: [
        {
          name: 'أموكسيسيلين',
          dose: '٥٠٠ ملغ',
          frequency: '٣ مرات يومياً',
          duration: 'لمدة ٧ أيام',
          warning: '',
          chemicalName: 'Amoxicillin Trihydrate',
          registryCode: 'EDA-REG-112233-AMX-03',
        },
        {
          name: 'بروفين',
          dose: '٤٠٠ ملغ',
          frequency: 'مرتين يومياً',
          duration: 'لمدة ٣ أيام',
          warning: 'تجنب تناوله على معدة فارغة',
          chemicalName: 'Ibuprofen',
          registryCode: 'EDA-REG-445566-IBU-02',
        },
      ],
    },
    {
      id: 3,
      name: 'وصفة يدوية - السكري',
      date: '١٠/٠٣/٢٠٢٥',
      source: 'manual',
      meds: [
        {
          name: 'إنسولين',
          dose: '١٠ وحدات',
          frequency: 'مرة يومياً',
          duration: 'مستمر',
          warning: '',
          chemicalName: '',
          registryCode: '',
        },
        {
          name: 'ميتفورمين',
          dose: '١٠٠٠ ملغ',
          frequency: 'مرتين يومياً',
          duration: 'مستمر',
          warning: '',
          chemicalName: 'Metformin Hydrochloride',
          registryCode: 'EDA-REG-204811-MET-02',
        },
        {
          name: 'أتورفاستاتين',
          dose: '٢٠ ملغ',
          frequency: 'مرة يومياً قبل النوم',
          duration: 'مستمر',
          warning: '',
          chemicalName: 'Atorvastatin Calcium',
          registryCode: 'EDA-REG-667788-ATV-01',
        },
        {
          name: 'أسبرين',
          dose: '٨١ ملغ',
          frequency: 'مرة يومياً',
          duration: 'مستمر',
          warning: 'تعارض محتمل مع وارفارين',
          chemicalName: 'Acetylsalicylic Acid',
          registryCode: 'EDA-REG-998877-ASP-04',
        },
      ],
    },
  ]);

  getById(id: number) {
    return this.prescriptions().find((p) => p.id === id);
  }

  delete(id: number) {
    this.prescriptions.update((list) => list.filter((p) => p.id !== id));
  }

  update(updated: any) {
    this.prescriptions.update((list) => list.map((p) => (p.id === updated.id ? updated : p)));
  }

  add(prescription: any) {
    this.prescriptions.update((list) => [...list, prescription]);
  }
}
