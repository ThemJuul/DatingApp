import { inject, Injectable } from '@angular/core';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { ConfirmDialogComponent } from '../modals/confirm-dialog/confirm-dialog.component';
import { map } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {
  bsModalRef?: BsModalRef;
  private modalService = inject(BsModalService);

  confirm(title = 'Confirmation', message = 'Are you sure you want to do this?', buttonOkText = 'Ok', buttonCancelText = 'Cancel') {
    const config: ModalOptions = {
      initialState: {
        title,
        message,
        buttonOkText,
        buttonCancelText
      }
    };

    this.bsModalRef = this.modalService.show(ConfirmDialogComponent, config);

    return this.bsModalRef.onHidden?.pipe(
      map(() => {
        if (this.bsModalRef?.content) {
          return this.bsModalRef.content.result;
        }
        
        return false;
      })
    )
  }
}
