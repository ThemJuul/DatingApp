import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { User } from '../_models/user';
import { Photo } from '../_models/photo';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;
  private httpClient = inject(HttpClient);

  getUserWithRoles() {
    return this.httpClient.get<User[]>(this.baseUrl + 'admin/users-with-roles');
  }

  updateUserRoles(username: string, roles: string[]) {
    return this.httpClient.post<string[]>(this.baseUrl + 'admin/edit-roles/' + username + '?roles=' + roles, {});
  }

  getPhotosForModeration() {
    return this.httpClient.get<Photo[]>(this.baseUrl + 'admin/photos-to-moderate');
  }

  approvePhoto(id: number) {
    return this.httpClient.post(this.baseUrl + 'admin/approve-photo/' + id, {});
  }

  rejectPhoto(id: number) {
    return this.httpClient.post(this.baseUrl + 'admin/reject-photo/' + id, {});
  }
}
