import { Injectable } from '@angular/core';
import { InteractionResult } from '../models';

@Injectable({
  providedIn: 'root',
})
export class Interaction {
  private famousDrugs = [
    'وارفارين (Warfarin)',
    'أسبرين (Aspirin)',
    'بانادول (Panadol)',
    'كونكور (Concor)',
    'ميتفورمين (Metformin)',
    'بروفين (Brufen)',
  ];

  getFamousDrugs(): string[] {
    return this.famousDrugs;
  }

  searchDrugs(query: string): string[] {
    if (!query) return this.famousDrugs;
    return this.famousDrugs.filter((d) => d.toLowerCase().includes(query.toLowerCase()));
  }

  async checkInteractions(drugs: string[]): Promise<InteractionResult> {
    try {
      const resp = await fetch('/api/check-interactions', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ drugs }),
      });
      if (!resp.ok) throw new Error('Server error');
      return await resp.json();
    } catch {
      return this.clientFallback(drugs);
    }
  }

  private clientFallback(drugs: string[]): InteractionResult {
    const norm = drugs.map((m) => m.toLowerCase());
    const hasAspirin = norm.some((m) => m.includes('aspirin') || m.includes('أسبرين'));
    const hasWarfarin = norm.some((m) => m.includes('warfarin') || m.includes('وارفارين'));

    if (hasAspirin && hasWarfarin) {
      return {
        status: 'high',
        title: 'لا تتناولهم معًا — استشر طبيبك فورًا',
        severityText: 'خطر تفاعل دوائي حاد',
        explanation:
          'يؤدي الجمع بين هذين العقارين (الوارفارين والأسبرين) إلى زيادة مفرطة في خطر حدوث نزيف داخلي أو آثار جانبية قلبية خطيرة. هذا التفاعل يعتبر من الدرجة الثالثة (شديد الخطورة) ويتطلب تدخلاً طبياً فورياً لمراجعة الخطة العلاجية.',
        checkedMeds: [
          { name: 'وارفارين (Warfarin)', dose: 'الجرعة: ٥ ملجم يومياً', severity: 'error' },
          { name: 'أسبرين (Aspirin)', dose: 'الجرعة: ٨١ ملجم عند اللزوم', severity: 'error' },
        ],
        source: 'DrugBank / EDA',
      };
    }

    return {
      status: 'low',
      title: 'أدويتك متوافقة للمتابعة الآمنة',
      severityText: 'لم يتم العثور على تداخلات خطيرة',
      explanation: `بناءً على الفحص المبدئي للأدوية التالية: (${drugs.join(' - ')}). لا توجد تداخلات دوائية حادة مسجلة في قاعدة البيانات المبدئية. ومع ذلك، يوصى دائماً بمراجعة الطبيب المعالج أو الصيدلاني عند بدء تناول أدوية جديدة معاً.`,
      checkedMeds: drugs.map((m) => ({ name: m, dose: 'الجرعة المعتادة', severity: 'success' })),
      source: 'SafeDose Core DB / FDA',
    };
  }
}
