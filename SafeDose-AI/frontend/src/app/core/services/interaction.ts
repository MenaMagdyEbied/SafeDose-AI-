import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DrugSearchResult, InteractionResult, CheckInteractionsPayload } from '../models';
@Injectable({
  providedIn: 'root',
})
export class Interaction {
  private readonly apiUrl = environment.apiUrl;
  private readonly http = inject(HttpClient);

  readonly searchResults = signal<DrugSearchResult[]>([]);
  readonly history = signal<InteractionResult[]>([]);

  async searchDrugs(query: string, limit = 10): Promise<DrugSearchResult[]> {
    if (!query) {
      this.searchResults.set([]);
      return [];
    }
    const results = await firstValueFrom(
      this.http.get<DrugSearchResult[]>(`${this.apiUrl}/drugs/search`, {
        params: { q: query, limit },
      }),
    );
    this.searchResults.set(results);
    return results;
  }

  async checkInteractions(payload: CheckInteractionsPayload): Promise<InteractionResult> {
    return firstValueFrom(
      this.http.post<InteractionResult>(`${this.apiUrl}/interactions/check`, payload),
    );
  }

  async getHistory(patientId: number, limit = 20, offset = 0): Promise<InteractionResult[]> {
    const list = await firstValueFrom(
      this.http.get<InteractionResult[]>(`${this.apiUrl}/interactions/history`, {
        params: { patientId, limit, offset },
      }),
    );
    this.history.set(list);
    return list;
  }

  getById(id: number): Promise<InteractionResult> {
    return firstValueFrom(this.http.get<InteractionResult>(`${this.apiUrl}/interactions/${id}`));
  }

  async deleteCheck(id: number): Promise<void> {
    await firstValueFrom(this.http.delete(`${this.apiUrl}/interactions/${id}`));
    this.history.update((list) => list.filter((h) => h.interactionCheckId !== id));
  }

  acknowledge(id: number): Promise<unknown> {
    return firstValueFrom(this.http.post(`${this.apiUrl}/interactions/${id}/acknowledge`, {}));
  }
}
