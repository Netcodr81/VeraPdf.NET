/*
 * This file is part of veraPDF Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.impl.pd.annotations;

import org.verapdf.as.ASAtom;
import org.verapdf.cos.COSObject;
import org.verapdf.gf.model.impl.pd.GFPDAnnot;
import org.verapdf.pd.PDResourcesHandler;
import org.verapdf.model.pdlayer.PDWidgetAnnot;
import org.verapdf.pd.PDAnnotation;
import org.verapdf.pd.PDPage;
import org.verapdf.pd.annotations.PDWidgetAnnotation;
import org.verapdf.pd.form.PDFormField;
import org.verapdf.pd.structure.PDStructElem;
import org.verapdf.tools.StaticResources;
import org.verapdf.tools.TaggedPDFConstants;
import org.verapdf.tools.TaggedPDFRoleMapHelper;

/**
 * Adapts a widget annotation, the page-level appearance of an interactive form field.
 *
 * <p>A widget may itself be a field or may inherit field data through {@code /Parent};
 * this distinction drives the alternate field name, field-widget flag, and signature
 * classification. For tagged PDF, the class also examines structure children for an
 * {@code Lbl} element. These properties support PDF/A and PDF/UA form and accessibility
 * checks. The model is created with the page's resources and does not operate the field.</p>
 *
 * <p><b>.NET porting notes:</b></p>
 * <ul>
 *   <li>Suggested type: {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Annotations.GFPDWidgetAnnot},
 *       implementing {@code VeraPdf.Model.PDLayer.IPDWidgetAnnot}.</li>
 *   <li>Use parser {@code PdfAnnotation}, {@code CosDictionary}, and structure abstractions;
 *       retain nullable values and distinguish a widget field from a child widget.</li>
 *   <li>Role-map and document context are currently static in the Java implementation;
 *       prefer injecting per-document state in C# to avoid cross-document leakage.</li>
 * </ul>
 *
 * @author Maxim Plushchov
 */
public class GFPDWidgetAnnot extends GFPDAnnot implements PDWidgetAnnot {

	/** Model type name for form-widget annotation rules. */
	public static final String WIDGET_ANNOTATION_TYPE = "PDWidgetAnnot";

	public GFPDWidgetAnnot(PDWidgetAnnotation annot, PDResourcesHandler pageResources, PDPage page) {
		super(annot, pageResources, page, WIDGET_ANNOTATION_TYPE);
	}

	@Override
	/** Returns the alternate field name on the field or, for child widgets, its parent. */
	public String getTU() {
		if (!isField()) {
			COSObject parent = ((PDWidgetAnnotation) simplePDObject).getParent();
			return parent != null ? parent.getStringKey(ASAtom.TU) : null;
		}
		return ((PDAnnotation) simplePDObject).getTU();
	}

	@Override
	/** Indicates whether this widget is a field itself or has a field parent. */
	public Boolean getisFieldWidget() {
		return isField() || ((PDWidgetAnnotation) simplePDObject).getParent() != null;
	}

	/** Tests the underlying object against the parser's form-field representation. */
	private boolean isField() {
		return PDFormField.isField(simplePDObject.getObject());
	}

	@Override
	/** Treats widgets whose field type is {@code /Sig} as signature widgets. */
	protected boolean isSignature() {
		return ASAtom.SIG.equals(((PDAnnotation) simplePDObject).getFT());
	}
	
	@Override
	/** Checks whether the parent structure element has a non-empty label child. */
	public Boolean getcontainsLbl() {
		TaggedPDFRoleMapHelper taggedPDFRoleMapHelper = StaticResources.getRoleMapHelper();
		if (taggedPDFRoleMapHelper == null) {
			return false;
		}
		COSObject parent = getParentDictionary();
		if (parent != null) {
			PDStructElem parentStructElem = new PDStructElem(parent);
			for (PDStructElem child : parentStructElem.getStructChildren()) {
				if (TaggedPDFConstants.LBL.equals(PDStructElem.getStructureElementStandardType(child)) && 
						!child.getChildren().isEmpty()) {
					return true;
				}
			}
		}
		return false;
	}

}
