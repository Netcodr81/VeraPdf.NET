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
import org.verapdf.cos.COSObjType;
import org.verapdf.cos.COSObject;
import org.verapdf.pd.PDResourcesHandler;
import org.verapdf.model.pdlayer.PDFileAttachmentAnnot;
import org.verapdf.pd.PDAnnotation;
import org.verapdf.pd.PDPage;

/**
 * Models a markup annotation that carries an embedded file specification.
 *
 * <p>A file-attachment annotation can associate an embedded file with a page; the
 * optional {@code /AFRelationship} describes how that file relates to the document.
 * PDF/A-3 permits only constrained associated-file use, so rules inspect both presence
 * and relationship instead of opening the attachment. This subclass reuses common
 * markup-annotation properties.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Annotations.GFPDFileAttachmentAnnot},
 * implementing {@code VeraPdf.Model.PDLayer.IPDFileAttachmentAnnot}. Use
 * {@code VeraPdf.Parser.Cos.CosDictionary} and {@code CosName} for {@code /FS} and
 * {@code /AFRelationship}; preserve nullable string results.</p>
 *
 * @author Maxim Plushchov
 */
public class GFPDFileAttachmentAnnot extends GFPDMarkupAnnot implements PDFileAttachmentAnnot {

	/** Model type name used by file-attachment validation rules. */
	public static final String FILE_ATTACHMENT_RUBBER_TYPE = "PDFileAttachmentAnnot";

	public GFPDFileAttachmentAnnot(PDAnnotation annot, PDResourcesHandler pageResources, PDPage page) {
		super(annot, pageResources, page, FILE_ATTACHMENT_RUBBER_TYPE);
	}

	@Override
	/** Reports whether the annotation dictionary contains the {@code /FS} entry. */
	public Boolean getcontainsFS() {
		return simplePDObject.knownKey(ASAtom.FS);
	}

	@Override
	/** Reads the relationship name only when {@code /FS} is a dictionary. */
	public String getAFRelationship() {
		COSObject fs = simplePDObject.getKey(ASAtom.FS);
		if (fs != null && fs.getType() == COSObjType.COS_DICT) {
			return fs.getNameKeyStringValue(ASAtom.AF_RELATIONSHIP);
		}
		return null;
	}

}
