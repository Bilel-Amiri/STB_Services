import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VirementItemComponent } from './virement-item.component';

describe('VirementItemComponent', () => {
  let component: VirementItemComponent;
  let fixture: ComponentFixture<VirementItemComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ VirementItemComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(VirementItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
