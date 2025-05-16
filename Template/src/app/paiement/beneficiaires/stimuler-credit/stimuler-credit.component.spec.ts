import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StimulerCreditComponent } from './stimuler-credit.component';

describe('StimulerCreditComponent', () => {
  let component: StimulerCreditComponent;
  let fixture: ComponentFixture<StimulerCreditComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ StimulerCreditComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(StimulerCreditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
