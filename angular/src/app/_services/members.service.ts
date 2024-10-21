import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { of, tap } from 'rxjs';
import { Photo } from '../_models/photo';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private httpClient = inject(HttpClient);
  baseUrl = environment.apiUrl;
  members = signal<Member[]>([]);

  getMembers() {
    return this.httpClient.get<Member[]>(this.baseUrl + 'users').subscribe({
      next: members => this.members.set(members)
    });
  }

  getMemberByUsername(username: string) {
    const member = this.members().find(x => x.username === username);

    if(member != undefined) {
      return of(member);
    }

    return this.httpClient.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(memberToUpdate: Member) {
    return this.httpClient.put(this.baseUrl + 'users', memberToUpdate).pipe(
      tap(() => {
        this.members.update(members => members.map(originalMember => originalMember.username === memberToUpdate.username 
          ? memberToUpdate : originalMember))
      })
    )
  }

  setMainPhoto(photo: Photo) {
    return this.httpClient.put(this.baseUrl + 'users/set-main-photo/' + photo.id, {}).pipe(
      tap(() => {
        this.members.update(members => members.map(m => {
          if (m.photos.includes(photo)) {
            m.photoUrl = photo.url;
          }

          return m;
        }))
      })
    );
  }

  deletePhoto(photo: Photo) {
    return this.httpClient.delete(this.baseUrl + 'users/delete-photo/' + photo.id).pipe(
      tap(() => {
        this.members.update(members => members.map(m => {
          if (m.photos.includes(photo)) {
            m.photos = m.photos.filter(x => x.id !== photo.id)
          }

          return m;
        }));
      })
    );
  }
}
