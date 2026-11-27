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

import org.verapdf.gf.model.impl.pd.GFPDAnnot;
import org.verapdf.pd.PDResourcesHandler;
import org.verapdf.model.pdlayer.PDSoundAnnot;
import org.verapdf.pd.PDAnnotation;
import org.verapdf.pd.PDPage;

/**
 * Represents a sound annotation that associates audio data with a page region.
 *
 * <p>Sound annotations can play audio when activated and are therefore active content
 * considered by PDF/A rules. The adapter exposes the sound subtype through the PD model
 * and inherits standard annotation geometry and appearance handling; it does not decode
 * or play the audio.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Annotations.GFPDSoundAnnot}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDSoundAnnot}. Reuse {@code GFPDAnnot} and parser
 * dictionary/stream abstractions, without adding audio playback dependencies.</p>
 *
 * @author Maxim Plushchov
 */
public class GFPDSoundAnnot extends GFPDAnnot implements PDSoundAnnot {

	/** Model type name for sound annotations. */
	public static final String SOUND_ANNOTATION_TYPE = "PDSoundAnnot";

	public GFPDSoundAnnot(PDAnnotation annot, PDResourcesHandler pageResources, PDPage page) {
		super(annot, pageResources, page, SOUND_ANNOTATION_TYPE);
	}

}
