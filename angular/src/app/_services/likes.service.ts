import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { setPaginatedResponse, setPaginationHeaders } from './pagination-helper';

@Injectable({
  providedIn: 'root'
})
export class LikesService {
  baseUrl = environment.apiUrl;
  private httpClient = inject(HttpClient);
  likeIds = signal<number[]>([]);
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);

  toggleLike(targetId: number) {
    return this.httpClient.post(`${this.baseUrl}likes/${targetId}`, {});
  }

  getLikes(predicate: string, pageNUmber: number, pageSize: number) {
    let params = setPaginationHeaders(pageNUmber, pageSize);

    params = params.append('predicate', predicate);

    return this.httpClient.get<Member[]>(`${this.baseUrl}likes`, {
        observe: 'response',
        params
      }).subscribe({
        next: response => setPaginatedResponse(response, this.paginatedResult)
      });
  }

  getLikeIds() {
    return this.httpClient.get<number[]>(`${this.baseUrl}likes/list`).subscribe({
      next: (ids: number[]) => this.likeIds.set(ids)
    });
  }

  clearStoredLikes() {
    this.paginatedResult.set(null);
  }
}
